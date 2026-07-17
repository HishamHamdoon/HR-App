import 'package:dio/dio.dart';

import '../auth/auth_controller.dart';
import '../auth/token_store.dart';
import '../config/app_config.dart';
import 'api_envelope.dart';

/// Wraps Dio with the bearer interceptor and envelope-aware calls.
///
/// Endpoints return the `{result, isSuccess, message}` envelope with a 200 even on
/// business failure, so [_send] validates `isSuccess` rather than trusting the status
/// code and throws [ApiException] on failure — callers deal in payloads, not envelopes.
///
/// On a 401 it tries to refresh the session once (A5) and replays the request; concurrent
/// 401s share a single refresh via [_refreshing].
class ApiClient {
  ApiClient({required this.tokenStore, required this.auth, Dio? dio})
    : _dio =
          dio ??
          Dio(
            BaseOptions(
              baseUrl: AppConfig.apiBaseUrl,
              connectTimeout: const Duration(seconds: 15),
              receiveTimeout: const Duration(seconds: 30),
              // Do not throw on non-2xx: the envelope parser needs the body of a 400/401
              // to read its message, and a raw-string error body would otherwise be lost.
              validateStatus: (_) => true,
            ),
          ) {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await tokenStore.readAccessToken();
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
      ),
    );
  }

  final Dio _dio;
  final TokenStore tokenStore;
  final AuthController auth;

  /// Single-flight guard: the in-progress refresh, shared by all callers that hit a 401
  /// at once so the refresh token is rotated exactly once.
  Future<bool>? _refreshing;

  Future<Object?> get(String path, {Map<String, dynamic>? query}) =>
      _send(() => _dio.get(path, queryParameters: query));

  Future<Object?> post(
    String path, {
    Object? body,
    Map<String, dynamic>? query,
  }) => _send(() => _dio.post(path, data: body, queryParameters: query));

  Future<Object?> put(String path, {Object? body}) =>
      _send(() => _dio.put(path, data: body));

  Future<Object?> patch(String path, {Map<String, dynamic>? query}) =>
      _send(() => _dio.patch(path, queryParameters: query));

  /// Attempts to exchange the stored refresh token for a new session. Used at startup to
  /// revive an expired access token. Returns true on success.
  Future<bool> refreshSession() => _refresh();

  /// Runs [request], refreshing-and-retrying once on a 401, then unwraps the envelope and
  /// returns `result` — or throws [ApiException] carrying the server's message.
  Future<Object?> _send(Future<Response<dynamic>> Function() request) async {
    var response = await _run(request);

    if (response.statusCode == 401 && await _refresh()) {
      // The onRequest interceptor picks up the new access token on the replay.
      response = await _run(request);
    }

    final envelope = parseEnvelope(response.statusCode, response.data);
    if (!envelope.isSuccess) {
      throw ApiException(
        envelope.message.isNotEmpty ? envelope.message : 'Request failed.',
        statusCode: response.statusCode,
      );
    }
    return envelope.result;
  }

  Future<Response<dynamic>> _run(
    Future<Response<dynamic>> Function() request,
  ) async {
    try {
      return await request();
    } on DioException catch (e) {
      throw ApiException(
        e.type == DioExceptionType.connectionError ||
                e.type == DioExceptionType.connectionTimeout
            ? 'Cannot reach the server. Check your connection.'
            : e.message ?? 'Network error.',
      );
    }
  }

  /// Refreshes the session, coalescing concurrent callers onto one attempt. On failure the
  /// session is cleared so the router sends the user to login.
  Future<bool> _refresh() {
    return _refreshing ??= _doRefresh().whenComplete(() => _refreshing = null);
  }

  Future<bool> _doRefresh() async {
    final refreshToken = await tokenStore.readRefreshToken();
    if (refreshToken == null || refreshToken.isEmpty) {
      await auth.logout();
      return false;
    }
    try {
      // Bypass _send (no bearer needed, must not recurse into refresh).
      final resp = await _dio.post(
        '/api/Auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      final envelope = parseEnvelope(resp.statusCode, resp.data);
      final result = envelope.result;
      final newAccess = result is Map ? result['token'] as String? : null;
      final newRefresh = result is Map
          ? result['refreshToken'] as String?
          : null;
      if (!envelope.isSuccess || newAccess == null || newAccess.isEmpty) {
        await auth.logout();
        return false;
      }
      await auth.onTokenRefreshed(newAccess, refreshToken: newRefresh);
      return true;
    } on DioException {
      // A network error is not an auth failure — keep the session and let the caller
      // surface the connectivity problem rather than logging the user out.
      return false;
    }
  }
}

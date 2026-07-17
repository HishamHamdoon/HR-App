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
        onResponse: (response, handler) async {
          // With no refresh endpoint yet (A5), a 401 means the session is over: clear it
          // so the router redirects to login. When A5 lands, this is where refresh-and-
          // retry-once goes, guarded by a mutex against concurrent 401s.
          if (response.statusCode == 401) {
            await auth.logout();
          }
          handler.next(response);
        },
      ),
    );
  }

  final Dio _dio;
  final TokenStore tokenStore;
  final AuthController auth;

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

  /// Runs [request], unwraps the envelope, and returns `result` — or throws
  /// [ApiException] carrying the server's message on any failure.
  Future<Object?> _send(Future<Response<dynamic>> Function() request) async {
    final Response<dynamic> response;
    try {
      response = await request();
    } on DioException catch (e) {
      throw ApiException(
        e.type == DioExceptionType.connectionError ||
                e.type == DioExceptionType.connectionTimeout
            ? 'Cannot reach the server. Check your connection.'
            : e.message ?? 'Network error.',
      );
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
}

import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/core/api/api_client.dart';
import 'package:hr_app/core/auth/auth_controller.dart';
import 'package:hr_app/core/auth/token_store.dart';

/// Fake Dio adapter: a protected path returns 401 until the caller presents the refreshed
/// bearer; the refresh endpoint hands back a new pair. Counts how many times refresh ran.
class _FakeAdapter implements HttpClientAdapter {
  int refreshCalls = 0;
  int protectedCalls = 0;

  ResponseBody _json(Map<String, dynamic> body, int status) =>
      ResponseBody.fromString(
        jsonEncode(body),
        status,
        headers: {
          Headers.contentTypeHeader: [Headers.jsonContentType],
        },
      );

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<List<int>>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    if (options.path == '/api/Auth/refresh') {
      refreshCalls++;
      return _json({
        'isSuccess': true,
        'result': {'token': 'new-access', 'refreshToken': 'rt-2'},
        'message': '',
      }, 200);
    }
    // Protected resource: 401 unless the refreshed token is presented.
    protectedCalls++;
    final auth = options.headers['Authorization'];
    if (auth == 'Bearer new-access') {
      return _json({
        'isSuccess': true,
        'result': {'ok': true},
        'message': '',
      }, 200);
    }
    return _json({
      'isSuccess': false,
      'result': null,
      'message': 'unauthorized',
    }, 401);
  }

  @override
  void close({bool force = false}) {}
}

ApiClient _client(TokenStore store, _FakeAdapter adapter) {
  final dio = Dio(BaseOptions(validateStatus: (_) => true))
    ..httpClientAdapter = adapter;
  return ApiClient(tokenStore: store, auth: AuthController(store), dio: dio);
}

void main() {
  final Map<String, String> mockStore = {};

  setUp(() {
    TestWidgetsFlutterBinding.ensureInitialized();
    mockStore.clear();
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(
          const MethodChannel('plugins.it_nomads.com/flutter_secure_storage'),
          (call) async {
            final args =
                (call.arguments as Map?)?.cast<String, dynamic>() ?? {};
            switch (call.method) {
              case 'write':
                mockStore[args['key'] as String] = args['value'] as String;
                return null;
              case 'read':
                return mockStore[args['key'] as String];
              case 'delete':
                mockStore.remove(args['key'] as String);
                return null;
              case 'readAll':
                return mockStore;
              case 'deleteAll':
                mockStore.clear();
                return null;
              case 'containsKey':
                return mockStore.containsKey(args['key'] as String);
            }
            return null;
          },
        );
  });

  test('401 refreshes once, retries, and succeeds', () async {
    final store = TokenStore();
    await store.saveAccessToken('old-access');
    await store.saveRefreshToken('rt-1');
    final adapter = _FakeAdapter();

    final result = await _client(store, adapter).get('/api/Employee/9');

    expect((result as Map)['ok'], true);
    expect(adapter.refreshCalls, 1);
    expect(adapter.protectedCalls, 2); // initial 401 + retry
    expect(await store.readAccessToken(), 'new-access'); // rotated in
    expect(await store.readRefreshToken(), 'rt-2');
  });

  test('concurrent 401s share a single refresh (single-flight)', () async {
    final store = TokenStore();
    await store.saveAccessToken('old-access');
    await store.saveRefreshToken('rt-1');
    final adapter = _FakeAdapter();
    final client = _client(store, adapter);

    await Future.wait([
      client.get('/api/a'),
      client.get('/api/b'),
      client.get('/api/c'),
    ]);

    // Three requests each hit 401, but only one refresh runs.
    expect(adapter.refreshCalls, 1);
  });

  test('no refresh token → session cleared, request fails', () async {
    final store = TokenStore();
    await store.saveAccessToken('old-access');
    // no refresh token stored
    final adapter = _FakeAdapter();

    await expectLater(
      _client(store, adapter).get('/api/Employee/9'),
      throwsA(isA<Object>()),
    );
    expect(adapter.refreshCalls, 0);
    expect(await store.readAccessToken(), isNull); // logged out
  });
}

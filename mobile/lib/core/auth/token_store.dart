import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Persists the JWT in the platform keystore (Keychain on iOS, EncryptedSharedPreferences
/// on Android). Never SharedPreferences — that is world-readable on a rooted device.
///
/// There is only an access token today; the API issues a 7-day JWT and has no refresh
/// endpoint yet (plan item A5). When A5 lands, the refresh token goes here too.
class TokenStore {
  TokenStore([FlutterSecureStorage? storage])
    : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;
  static const _accessTokenKey = 'access_token';

  Future<void> saveAccessToken(String token) =>
      _storage.write(key: _accessTokenKey, value: token);

  Future<String?> readAccessToken() => _storage.read(key: _accessTokenKey);

  Future<void> clear() => _storage.delete(key: _accessTokenKey);
}

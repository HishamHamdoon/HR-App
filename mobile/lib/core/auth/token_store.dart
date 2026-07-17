import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Persists the JWT (and refresh token) in the platform keystore (Keychain on iOS,
/// EncryptedSharedPreferences on Android). Never SharedPreferences — that is
/// world-readable on a rooted device.
///
/// The refresh token (A5) lets the app get a new access token silently when the current
/// one is rejected, and is rotated by the server on every use.
class TokenStore {
  TokenStore([FlutterSecureStorage? storage])
    : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;
  static const _accessTokenKey = 'access_token';
  static const _refreshTokenKey = 'refresh_token';

  Future<void> saveAccessToken(String token) =>
      _storage.write(key: _accessTokenKey, value: token);

  Future<String?> readAccessToken() => _storage.read(key: _accessTokenKey);

  Future<void> saveRefreshToken(String token) =>
      _storage.write(key: _refreshTokenKey, value: token);

  Future<String?> readRefreshToken() => _storage.read(key: _refreshTokenKey);

  Future<void> clear() async {
    await _storage.delete(key: _accessTokenKey);
    await _storage.delete(key: _refreshTokenKey);
  }
}

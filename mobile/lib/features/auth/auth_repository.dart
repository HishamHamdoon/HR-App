import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';

/// Auth calls against Emp.Api. Returns raw data or throws [ApiException]; it does not
/// touch session state — [AuthController] owns that.
class AuthRepository {
  AuthRepository(this._api);

  final ApiClient _api;

  /// Exchanges credentials for a JWT. `username` is the login identifier (an email for
  /// most users, but `admin` for the seeded admin — not always the email).
  Future<String> login(String username, String password) async {
    final result = await _api.post(
      '/api/Auth/login',
      body: {'username': username, 'password': password},
    );
    final token = result is Map ? result['token'] as String? : null;
    if (token == null || token.isEmpty) {
      throw const ApiException('Sign-in did not return a token.');
    }
    return token;
  }

  /// Changes the password. Succeeds silently; throws with the server's message on
  /// failure (wrong current password, policy violations). Note: the API issues NO new
  /// token here, so a forced first-login change must be followed by a fresh login — the
  /// stored token still says MustChangePassword until then.
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    await _api.post(
      '/api/Auth/change-password',
      body: {'currentPassword': currentPassword, 'newPassword': newPassword},
    );
  }
}

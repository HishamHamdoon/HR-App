import 'package:flutter/foundation.dart';

import 'jwt.dart';
import 'token_store.dart';

/// Holds the current session and notifies the router when it changes.
///
/// A plain [ChangeNotifier] rather than a Riverpod notifier because go_router's
/// `refreshListenable` takes a [Listenable]; the provider in `providers.dart` exposes it
/// to the rest of the app.
class AuthController extends ChangeNotifier {
  AuthController(this._tokenStore);

  final TokenStore _tokenStore;

  JwtClaims? _claims;
  bool _bootstrapped = false;

  JwtClaims? get claims => _claims;

  /// False until [bootstrap] has read storage, so the router can hold on a splash
  /// instead of flashing the login screen before the stored token is loaded.
  bool get isBootstrapped => _bootstrapped;

  bool get isLoggedIn => _claims != null && !_claims!.isExpired;

  bool get mustChangePassword => _claims?.mustChangePassword ?? false;

  /// Load any persisted token on launch. Call once at startup.
  Future<void> bootstrap() async {
    final token = await _tokenStore.readAccessToken();
    _claims = JwtClaims.tryParse(token);
    if (_claims != null && _claims!.isExpired) {
      await _tokenStore.clear();
      _claims = null;
    }
    _bootstrapped = true;
    notifyListeners();
  }

  /// Persist a freshly issued token and adopt its claims.
  Future<void> onLoggedIn(String token) async {
    await _tokenStore.saveAccessToken(token);
    _claims = JwtClaims.tryParse(token);
    notifyListeners();
  }

  /// The password-change endpoint clears the server-side flag; refresh local claims from
  /// the new token so the router stops pinning to the change-password screen.
  Future<void> onTokenRefreshed(String token) => onLoggedIn(token);

  Future<void> logout() async {
    await _tokenStore.clear();
    _claims = null;
    notifyListeners();
  }
}

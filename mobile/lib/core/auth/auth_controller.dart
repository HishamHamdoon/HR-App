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
  String? _notice;

  JwtClaims? get claims => _claims;

  /// False until startup finishes (including any silent refresh), so the router can hold
  /// on a splash instead of flashing the login screen.
  bool get isBootstrapped => _bootstrapped;

  bool get isLoggedIn => _claims != null && !_claims!.isExpired;

  bool get mustChangePassword => _claims?.mustChangePassword ?? false;

  /// Reads the persisted access token and adopts it if still valid. Does NOT mark
  /// bootstrap complete — startup may still attempt a refresh (see [hasRefreshToken] and
  /// [markBootstrapped]) before the router is allowed off the splash.
  Future<void> bootstrap() async {
    final token = await _tokenStore.readAccessToken();
    final claims = JwtClaims.tryParse(token);
    _claims = (claims != null && !claims.isExpired) ? claims : null;
  }

  Future<bool> hasRefreshToken() async =>
      (await _tokenStore.readRefreshToken())?.isNotEmpty ?? false;

  Future<String?> readRefreshToken() => _tokenStore.readRefreshToken();

  /// Marks startup complete and lets the router evaluate the final state.
  void markBootstrapped() {
    _bootstrapped = true;
    notifyListeners();
  }

  /// Persist a freshly issued token pair and adopt the access token's claims.
  Future<void> onLoggedIn(String token, {String? refreshToken}) async {
    await _tokenStore.saveAccessToken(token);
    if (refreshToken != null && refreshToken.isNotEmpty) {
      await _tokenStore.saveRefreshToken(refreshToken);
    }
    _claims = JwtClaims.tryParse(token);
    notifyListeners();
  }

  /// After a password change (which clears MustChangePassword server-side) or a silent
  /// refresh: adopt the new token so the router re-evaluates.
  Future<void> onTokenRefreshed(String token, {String? refreshToken}) =>
      onLoggedIn(token, refreshToken: refreshToken);

  Future<void> logout() async {
    await _tokenStore.clear();
    _claims = null;
    notifyListeners();
  }

  /// Ends the session and leaves a one-shot message for the login screen to show — used
  /// when we sign the user out deliberately (e.g. after a forced password change, since
  /// the API issues no fresh token and the stored one still says MustChangePassword).
  Future<void> logoutWithNotice(String notice) async {
    _notice = notice;
    await logout();
  }

  /// Returns the pending notice and clears it, so it shows only once.
  String? consumeNotice() {
    final n = _notice;
    _notice = null;
    return n;
  }
}

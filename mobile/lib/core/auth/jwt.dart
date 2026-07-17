import 'dart:convert';

/// The claims Emp.Api's JwtTokenGenerator puts in the token, decoded client-side.
///
/// These drive routing and UI gating without a round-trip: `isManager` shows the team
/// screens, `mustChangePassword` pins the user to the change-password screen (mirroring
/// the server's LicenseEnforcementMiddleware). The token is NOT verified here — the API
/// verifies its own signature. This only reads claims to decide what to show.
class JwtClaims {
  const JwtClaims({
    required this.employeeId,
    required this.isManager,
    required this.mustChangePassword,
    required this.email,
    required this.name,
    required this.roles,
    required this.theme,
    required this.calendar,
    required this.language,
    required this.expiresAt,
  });

  final int? employeeId;
  final bool isManager;
  final bool mustChangePassword;
  final String email;
  final String name;
  final List<String> roles;
  final String theme;
  final String calendar;
  final String language;
  final DateTime? expiresAt;

  bool get isAdmin => roles.contains('Admin');

  /// True once the token is past its `exp`. With only a 7-day token and no refresh
  /// endpoint (A5), an expired token means re-login.
  bool get isExpired {
    final exp = expiresAt;
    return exp != null && DateTime.now().toUtc().isAfter(exp);
  }

  /// Returns null for a malformed token rather than throwing — a corrupt stored token
  /// should route to login, not crash the app.
  static JwtClaims? tryParse(String? token) {
    if (token == null || token.isEmpty) return null;
    final parts = token.split('.');
    if (parts.length != 3) return null;

    final Map<String, dynamic> c;
    try {
      final payload = utf8.decode(
        base64Url.decode(base64Url.normalize(parts[1])),
      );
      final decoded = jsonDecode(payload);
      if (decoded is! Map<String, dynamic>) return null;
      c = decoded;
    } catch (_) {
      return null;
    }

    final exp = c['exp'];
    return JwtClaims(
      employeeId: int.tryParse('${c['EmployeeId'] ?? ''}'),
      isManager: '${c['IsManager']}'.toLowerCase() == 'true',
      mustChangePassword: '${c['MustChangePassword']}'.toLowerCase() == 'true',
      email: '${c['email'] ?? ''}',
      name: '${c['name'] ?? ''}',
      roles: _roles(c['role']),
      theme: '${c['PreferredTheme'] ?? 'light'}',
      calendar: '${c['Calendar'] ?? 'Gregorian'}',
      language: '${c['Lang'] ?? 'en'}',
      expiresAt: exp is int
          ? DateTime.fromMillisecondsSinceEpoch(exp * 1000, isUtc: true)
          : null,
    );
  }

  /// The `role` claim is a single string when there is one role and a list when there
  /// are several — the standard .NET serialization quirk.
  static List<String> _roles(Object? raw) {
    if (raw is List) return raw.map((e) => '$e').toList();
    if (raw is String && raw.isNotEmpty) return [raw];
    return const [];
  }
}

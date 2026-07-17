/// Build-time configuration.
///
/// The base URL must be supplied with `--dart-define`, e.g.
///
/// ```
/// flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5172
/// ```
///
/// The default targets the Android emulator, which reaches the host machine at
/// 10.0.2.2 — `localhost` inside the emulator is the emulator itself, and a physical
/// device can reach neither. This is the single most common way to waste an afternoon
/// on a Flutter/ASP.NET pairing, so it is a defined constant rather than a literal
/// buried in the Dio setup.
class AppConfig {
  const AppConfig._();

  /// Root of the API, no trailing slash. Emp.Api serves http on 5172, https on 7031.
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5172',
  );

  /// Fail fast rather than issue requests against an empty host.
  static bool get isConfigured => apiBaseUrl.isNotEmpty;
}

# HR-App mobile

Flutter client for the HR-App API (`Emp.Api`). Employee + manager self-service: login,
profile, leave (balance / history / request with attachment), manager approvals, in-app
notifications, and payslips. Architecture and API notes live in [PLAN.md](PLAN.md);
release-hardening status in [SECURITY.md](SECURITY.md).

## Requirements

- Flutter 3.44.x / Dart 3.12.x (pinned in CI).
- A running `Emp.Api` instance. Some features need API changes that ship on their own
  branches: payslips need the employee salary/payroll endpoints (A4), and staying signed in
  past an access-token expiry needs refresh tokens (A5).

## Configure the API base URL

The base URL is a build-time constant supplied with `--dart-define`. The default targets the
**Android emulator**, which reaches the host machine at `10.0.2.2` (not `localhost`, which is
the emulator itself). A physical device or iOS simulator needs a different value.

```bash
# Android emulator against a local API on http (default)
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5172

# Physical device on the same LAN
flutter run --dart-define=API_BASE_URL=http://<your-machine-ip>:5172

# Production
flutter run --release --dart-define=API_BASE_URL=https://api.example.com
```

Release builds are HTTPS-only (cleartext http is allowed in debug only).

## Develop

```bash
flutter pub get
flutter gen-l10n     # regenerate localisations after editing lib/l10n/*.arb
flutter analyze
flutter test
dart format .
```

## Release build

1. Create a keystore and `android/key.properties` (see `android/key.properties.template`).
2. Work through [SECURITY.md](SECURITY.md) — obfuscation, cert pinning, store metadata.
3. `flutter build appbundle --release --dart-define=API_BASE_URL=https://…`

## Gotcha: broken `pwsh` on the PATH

If a `pwsh` (PowerShell) shim on the PATH fails to start (e.g. a .NET global tool needing a
runtime that isn't installed), `flutter` and `dart` fail early because their launch scripts
prefer `pwsh`. Fix the tool (`dotnet tool uninstall --global powershell` or install the
required runtime), or remove that directory from the PATH for the shell running Flutter.

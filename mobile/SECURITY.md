# Mobile hardening checklist

Status of the release-hardening work (Phase 9). Items marked **needs device/human** could
not be build-verified in the development environment (no Android SDK, signing keys, or
store accounts here) — they must be validated before shipping.

## Done in code

- **Token storage** — access and refresh tokens live in the platform keystore via
  `flutter_secure_storage` (Keychain / EncryptedSharedPreferences), never SharedPreferences.
- **Refresh-token rotation** — the server rotates on every use and stores only a hash;
  the client refreshes-and-retries once on 401 with single-flight coalescing (Phase 8 / A5).
- **Leave attachments** are fetched through an authorized endpoint, not served statically
  (Phase 1 / A3).
- **Cleartext traffic** is disabled in release (`usesCleartextTraffic="false"` in the main
  manifest) and permitted only in the debug manifest, for local `http://10.0.2.2` dev.
- **INTERNET permission** is declared in the main manifest so release builds have network.
- **Release signing** reads `android/key.properties` (git-ignored) and falls back to debug
  signing when absent. Keystores and `key.properties` are git-ignored.

## Needs device / human before release

- [ ] **Signing keystore** — create it (`keytool …`, see `android/key.properties.template`),
      store it and its passwords in a secret manager, never in git.
- [ ] **Code shrinking / obfuscation** — `isMinifyEnabled` / `isShrinkResources` are OFF in
      `build.gradle.kts`. Turn on, then test a `--release` build of every screen (R8 can strip
      classes the plugins reach reflectively). Also build Dart with
      `--obfuscate --split-debug-info=build/symbols`.
- [ ] **Certificate pinning** — not implemented (the production API host/cert is unknown
      here). Add a Dio `badCertificateCallback` or a pinning interceptor keyed to the prod
      cert's SPKI SHA-256 once known. Pin a backup key too, and have a rotation plan.
- [ ] **Root / jailbreak posture** — decide the policy (warn vs. block) and, if wanted, add a
      detection package. Treat detection as advisory, not a security boundary.
- [ ] **iOS App Transport Security** — the default (HTTPS-only) is correct for release. For
      local http dev on the simulator, add a scoped `NSAppTransportSecurity` exception (e.g.
      `NSAllowsLocalNetworking`) to a debug config, not the shipping `Info.plist`.
- [ ] **Base URL per flavor** — ship prod pointing at the real HTTPS host via
      `--dart-define=API_BASE_URL=…` (see README). The default `10.0.2.2` is emulator-only.
- [ ] **Store metadata** — app icons, screenshots, privacy policy, data-safety declaration
      (the app stores auth tokens and reads salary data), and account-deletion path.
- [ ] **Remove the `pwsh` PATH hazard** on build machines (see README) so `flutter`/`dart`
      run cleanly.

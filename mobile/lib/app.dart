import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/providers.dart';
import 'core/router.dart';
import 'core/theme.dart';
import 'l10n/app_localizations.dart';

/// Root widget. Kicks off the token bootstrap once, then hands off to the router, which
/// decides splash / login / change-password / home from auth state.
class HrApp extends ConsumerStatefulWidget {
  const HrApp({super.key});

  @override
  ConsumerState<HrApp> createState() => _HrAppState();
}

class _HrAppState extends ConsumerState<HrApp> {
  @override
  void initState() {
    super.initState();
    // After first frame so the provider container is ready.
    WidgetsBinding.instance.addPostFrameCallback((_) => _boot());
  }

  /// Load the stored session; if the access token is gone/expired but a refresh token
  /// remains, silently refresh before letting the router off the splash — so a returning
  /// user isn't bounced to login within the refresh token's lifetime.
  Future<void> _boot() async {
    final auth = ref.read(authControllerProvider);
    await auth.bootstrap();
    if (!auth.isLoggedIn && await auth.hasRefreshToken()) {
      await ref.read(apiClientProvider).refreshSession();
    }
    auth.markBootstrapped();
  }

  @override
  Widget build(BuildContext context) {
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      onGenerateTitle: (context) => AppL10n.of(context).appTitle,
      theme: AppTheme.light,
      darkTheme: AppTheme.dark,
      routerConfig: router,
      localizationsDelegates: const [
        AppL10n.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: AppL10n.supportedLocales,
      // Directionality (RTL for ar) follows the active locale automatically.
    );
  }
}

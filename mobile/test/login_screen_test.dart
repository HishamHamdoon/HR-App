import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/auth/login_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

/// Builds the login screen for real (not just static analysis) to confirm the widget
/// layer, the generated localizations, and RTL for Arabic all wire up.
Widget _host(Locale locale) => ProviderScope(
  child: MaterialApp(
    locale: locale,
    localizationsDelegates: const [
      AppL10n.delegate,
      GlobalMaterialLocalizations.delegate,
      GlobalWidgetsLocalizations.delegate,
      GlobalCupertinoLocalizations.delegate,
    ],
    supportedLocales: AppL10n.supportedLocales,
    home: const LoginScreen(),
  ),
);

void main() {
  testWidgets('renders English strings', (tester) async {
    await tester.pumpWidget(_host(const Locale('en')));
    expect(find.text('Sign in'), findsWidgets);
    expect(find.text('Username'), findsOneWidget);
  });

  testWidgets('renders Arabic strings with RTL direction', (tester) async {
    await tester.pumpWidget(_host(const Locale('ar')));
    await tester.pumpAndSettle();
    expect(find.text('اسم المستخدم'), findsOneWidget);
    expect(
      Directionality.of(tester.element(find.byType(TextField).first)),
      TextDirection.rtl,
    );
  });
}

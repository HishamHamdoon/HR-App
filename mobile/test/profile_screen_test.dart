import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/profile/employee_profile.dart';
import 'package:hr_app/features/profile/profile_providers.dart';
import 'package:hr_app/features/profile/profile_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

const _screen = MaterialApp(
  localizationsDelegates: [
    AppL10n.delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
  ],
  supportedLocales: AppL10n.supportedLocales,
  home: ProfileScreen(),
);

final _sample = EmployeeProfile.fromJson({
  'id': 7,
  'name': 'Aisha Khan',
  'email': 'aisha@corp.com',
  'phone': '0100',
  'address': '1 Main St',
  'isActive': true,
  'birthDate': '1990-05-01',
  'hireDate': '2020-01-15',
  'departmentName': 'Finance',
  'jobTitleTitle': 'Accountant',
  'countryName': 'Egypt',
  'manager': 'Omar',
});

void main() {
  testWidgets('renders the profile and an edit affordance', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [myProfileProvider.overrideWith((ref) async => _sample)],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Aisha Khan'), findsOneWidget);
    expect(find.text('aisha@corp.com'), findsOneWidget);
    expect(find.text('Finance'), findsOneWidget);
    expect(find.text('Edit profile'), findsOneWidget); // FAB
  });

  testWidgets('shows an error state with retry', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          myProfileProvider.overrideWith(
            (ref) async => throw Exception('boom'),
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.textContaining('boom'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });
}

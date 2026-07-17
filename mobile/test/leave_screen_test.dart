import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/leave/leave_models.dart';
import 'package:hr_app/features/leave/leave_providers.dart';
import 'package:hr_app/features/leave/leave_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

const _screen = MaterialApp(
  localizationsDelegates: [
    AppL10n.delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
  ],
  supportedLocales: AppL10n.supportedLocales,
  home: LeaveScreen(),
);

void main() {
  testWidgets('balance tab renders rows with remaining days', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          myLeaveBalanceProvider.overrideWith(
            (ref) async => [
              const LeaveBalance(
                leaveTypeId: 1,
                leaveType: 'Annual',
                entitlement: 21,
                taken: 5,
                remaining: 16,
              ),
            ],
          ),
          myLeavesProvider.overrideWith((ref) async => []),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Annual'), findsOneWidget);
    expect(find.textContaining('16'), findsWidgets); // remaining
  });

  testWidgets('history tab shows an empty state', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          myLeaveBalanceProvider.overrideWith((ref) async => []),
          myLeavesProvider.overrideWith((ref) async => []),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    // Switch to the History tab.
    await tester.tap(find.text('History'));
    await tester.pumpAndSettle();
    expect(find.text('You have no leave requests yet.'), findsOneWidget);
  });
}

import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/manager/manager_providers.dart';
import 'package:hr_app/features/manager/team_leave.dart';
import 'package:hr_app/features/manager/team_leaves_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

const _screen = MaterialApp(
  localizationsDelegates: [
    AppL10n.delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
  ],
  supportedLocales: AppL10n.supportedLocales,
  home: TeamLeavesScreen(),
);

TeamLeave _leave(int id, String status, String name) => TeamLeave.fromJson({
  'id': id,
  'startDate': '2026-08-0${id}T00:00:00',
  'endDate': '2026-08-0${id}T00:00:00',
  'status': status,
  'note': '',
  'employeeName': name,
  'leaveType': 'Annual',
});

void main() {
  testWidgets('pending tab shows only pending, with approve/reject actions', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          teamLeavesProvider.overrideWith(
            (ref) async => [
              _leave(1, 'PENDING', 'Sara'),
              _leave(2, 'APPROVED', 'Omar'),
            ],
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    // Pending tab is first: Sara (pending) shows, Omar (approved) does not.
    expect(find.text('Sara'), findsOneWidget);
    expect(find.text('Omar'), findsNothing);
    expect(find.text('Approve'), findsOneWidget);
    expect(find.text('Reject'), findsOneWidget);
  });

  testWidgets('pending empty state', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          teamLeavesProvider.overrideWith(
            (ref) async => [_leave(2, 'APPROVED', 'Omar')],
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();
    expect(
      find.text('No leaves are waiting for your decision.'),
      findsOneWidget,
    );
  });
}

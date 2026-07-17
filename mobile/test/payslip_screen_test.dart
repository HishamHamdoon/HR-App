import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/payslip/payslip_models.dart';
import 'package:hr_app/features/payslip/payslip_providers.dart';
import 'package:hr_app/features/payslip/payslip_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

const _screen = MaterialApp(
  localizationsDelegates: [
    AppL10n.delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
  ],
  supportedLocales: AppL10n.supportedLocales,
  home: PayslipScreen(),
);

void main() {
  testWidgets('shows salary breakdown and payroll history', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          mySalaryProvider.overrideWith(
            (ref) async => SalaryInfo.fromJson({
              'id': 2,
              'basicSalary': 10000,
              'allowances': 2000,
              'deductions': 500,
              'netSalary': 11500,
              'effectiveDate': '2026-07-01T00:00:00',
            }),
          ),
          myPayrollsProvider.overrideWith(
            (ref) async => [
              Payslip.fromJson({
                'id': 3,
                'grossSalary': 12000,
                'deductions': 500,
                'netSalary': 11500,
                'salaryMonth': '2026-07-01T00:00:00',
                'isPaid': true,
                'generatedAt': '2026-07-02T10:00:00',
              }),
            ],
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Current salary'), findsOneWidget);
    expect(find.textContaining('11,500'), findsWidgets); // net, formatted
    expect(find.text('Paid'), findsOneWidget);
  });

  testWidgets('empty states for no salary and no payrolls', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          mySalaryProvider.overrideWith((ref) async => null),
          myPayrollsProvider.overrideWith((ref) async => []),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(
      find.text('No salary has been configured for your account.'),
      findsOneWidget,
    );
    expect(find.text('You have no payroll records yet.'), findsOneWidget);
  });
}

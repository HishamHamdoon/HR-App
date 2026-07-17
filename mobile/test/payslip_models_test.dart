import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/payslip/payslip_models.dart';

void main() {
  test('SalaryInfo maps SalaryDto incl. decimal money', () {
    final s = SalaryInfo.fromJson({
      'id': 2,
      'employeeId': 9,
      'employeeName': 'X',
      'basicSalary': 10000.00,
      'allowances': 2000.00,
      'deductions': 500.00,
      'netSalary': 11500.00,
      'effectiveDate': '2026-07-01T00:00:00',
    });
    expect(s.basicSalary, 10000);
    expect(s.netSalary, 11500);
    expect(s.effectiveDate, DateTime(2026, 7, 1));
  });

  test('Payslip maps PayrollDto', () {
    final p = Payslip.fromJson({
      'id': 3,
      'grossSalary': 12000,
      'deductions': 500,
      'netSalary': 11500,
      'salaryMonth': '2026-07-01T00:00:00',
      'isPaid': true,
      'generatedAt': '2026-07-02T10:00:00',
    });
    expect(p.grossSalary, 12000);
    expect(p.isPaid, isTrue);
    expect(p.salaryMonth, DateTime(2026, 7, 1));
  });

  test('tolerates integer money and missing fields', () {
    final s = SalaryInfo.fromJson({'id': 1, 'basicSalary': 5000});
    expect(s.basicSalary, 5000);
    expect(s.allowances, 0);
    expect(s.effectiveDate, isNull);
  });
}

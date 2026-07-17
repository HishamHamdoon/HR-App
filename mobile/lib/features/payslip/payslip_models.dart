// Payslip domain models, decoded from Emp.Api's A4 self-service endpoints.

/// The caller's current salary (`GET /api/Salary/mine` → SalaryDto). `netSalary` is
/// computed server-side (basic + allowances − deductions).
class SalaryInfo {
  const SalaryInfo({
    required this.id,
    required this.basicSalary,
    required this.allowances,
    required this.deductions,
    required this.netSalary,
    required this.effectiveDate,
  });

  final int id;
  final double basicSalary;
  final double allowances;
  final double deductions;
  final double netSalary;
  final DateTime? effectiveDate;

  factory SalaryInfo.fromJson(Map<String, dynamic> j) => SalaryInfo(
    id: (j['id'] as num?)?.toInt() ?? 0,
    basicSalary: _num(j['basicSalary']),
    allowances: _num(j['allowances']),
    deductions: _num(j['deductions']),
    netSalary: _num(j['netSalary']),
    effectiveDate: _date(j['effectiveDate']),
  );
}

/// One generated payroll record (`GET /api/Payrolls/mine` → PayrollDto).
class Payslip {
  const Payslip({
    required this.id,
    required this.grossSalary,
    required this.deductions,
    required this.netSalary,
    required this.salaryMonth,
    required this.isPaid,
    required this.generatedAt,
  });

  final int id;
  final double grossSalary;
  final double deductions;
  final double netSalary;
  final DateTime? salaryMonth;
  final bool isPaid;
  final DateTime? generatedAt;

  factory Payslip.fromJson(Map<String, dynamic> j) => Payslip(
    id: (j['id'] as num?)?.toInt() ?? 0,
    grossSalary: _num(j['grossSalary']),
    deductions: _num(j['deductions']),
    netSalary: _num(j['netSalary']),
    salaryMonth: _date(j['salaryMonth']),
    isPaid: j['isPaid'] == true,
    generatedAt: _date(j['generatedAt']),
  );
}

double _num(Object? v) => (v as num?)?.toDouble() ?? 0;

DateTime? _date(Object? v) =>
    (v is String && v.isNotEmpty) ? DateTime.tryParse(v) : null;

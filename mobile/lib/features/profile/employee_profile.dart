/// The caller's own employee record, decoded from Emp.Api's `EmployeeViewDto`
/// (`GET /api/Employee/{id}`).
///
/// Only `phone` and `address` are editable by the employee (via
/// `PUT /api/Employee/my-profile`); everything else is read-only here and maintained by
/// Admin on the web. Dates arrive as `DateOnly` — "yyyy-MM-dd" strings on the wire.
class EmployeeProfile {
  const EmployeeProfile({
    required this.id,
    required this.name,
    required this.email,
    required this.phone,
    required this.address,
    required this.isActive,
    required this.birthDate,
    required this.hireDate,
    required this.leavingDate,
    required this.departmentName,
    required this.jobTitle,
    required this.country,
    required this.manager,
  });

  final int id;
  final String name;
  final String email;
  final String phone;
  final String address;
  final bool isActive;
  final DateTime? birthDate;
  final DateTime? hireDate;
  final DateTime? leavingDate;
  final String departmentName;
  final String jobTitle;
  final String country;
  final String manager;

  factory EmployeeProfile.fromJson(Map<String, dynamic> j) {
    return EmployeeProfile(
      id: (j['id'] as num?)?.toInt() ?? 0,
      name: _str(j['name']),
      email: _str(j['email']),
      phone: _str(j['phone']),
      address: _str(j['address']),
      // The DTO property is `isActive` (lower i); camelCase policy leaves it unchanged.
      isActive: j['isActive'] == true,
      birthDate: _date(j['birthDate']),
      hireDate: _date(j['hireDate']),
      leavingDate: _date(j['leavingDate']),
      departmentName: _str(j['departmentName']),
      jobTitle: _str(j['jobTitleTitle']),
      country: _str(j['countryName']),
      manager: _str(j['manager']),
    );
  }

  static String _str(Object? v) => v == null ? '' : '$v';

  static DateTime? _date(Object? v) =>
      (v is String && v.isNotEmpty) ? DateTime.tryParse(v) : null;
}

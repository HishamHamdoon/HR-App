import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/profile/employee_profile.dart';

/// Pins the mapping from Emp.Api's EmployeeViewDto JSON — the camelCase field names and
/// the DateOnly "yyyy-MM-dd" strings this app has to bind to.
void main() {
  group('EmployeeProfile.fromJson', () {
    test('maps a full record', () {
      final p = EmployeeProfile.fromJson({
        'id': 42,
        'name': 'Aisha Khan',
        'email': 'aisha@corp.com',
        'phone': '0100',
        'address': '1 Main St',
        'isActive': true,
        'birthDate': '1990-05-01',
        'hireDate': '2020-01-15',
        'leavingDate': null,
        'departmentName': 'Finance',
        'jobTitleTitle': 'Accountant',
        'countryName': 'Egypt',
        'manager': 'Omar',
      });

      expect(p.id, 42);
      expect(p.name, 'Aisha Khan');
      expect(p.jobTitle, 'Accountant'); // from jobTitleTitle
      expect(p.isActive, isTrue);
      expect(p.birthDate, DateTime(1990, 5, 1));
      expect(p.hireDate, DateTime(2020, 1, 15));
      expect(p.leavingDate, isNull);
    });

    test('tolerates missing and null fields', () {
      final p = EmployeeProfile.fromJson({'id': 1, 'name': 'X'});
      expect(p.email, '');
      expect(p.phone, '');
      expect(p.manager, '');
      expect(p.birthDate, isNull);
      expect(p.isActive, isFalse);
    });

    test('unparseable date becomes null, not a throw', () {
      final p = EmployeeProfile.fromJson({
        'id': 1,
        'name': 'X',
        'hireDate': 'not-a-date',
      });
      expect(p.hireDate, isNull);
    });
  });
}

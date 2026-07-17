import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/manager/team_leave.dart';

void main() {
  test('maps the manager projection', () {
    final t = TeamLeave.fromJson({
      'id': 5,
      'startDate': '2026-08-01T00:00:00',
      'endDate': '2026-08-03T00:00:00',
      'status': 'pending',
      'note': 'family',
      'employeeName': 'Sara',
      'leaveType': 'Annual',
    });
    expect(t.id, 5);
    expect(t.employeeName, 'Sara');
    expect(t.leaveType, 'Annual');
    expect(t.status, 'PENDING');
    expect(t.isPending, isTrue);
    expect(t.startDate, DateTime(2026, 8, 1));
  });

  test('non-pending status is not pending', () {
    final t = TeamLeave.fromJson({'id': 1, 'status': 'APPROVED'});
    expect(t.isPending, isFalse);
    expect(t.employeeName, '');
  });
}

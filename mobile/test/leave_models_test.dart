import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/leave/leave_models.dart';

void main() {
  test('LeaveType maps from LeaveTypesViewDto', () {
    final t = LeaveType.fromJson({
      'id': 3,
      'name': 'Sick leave',
      'note': 'n',
      'maxDays': 15,
      'minDays': 1,
      'isAttachmentRequired': true,
      'isActive': true,
    });
    expect(t.name, 'Sick leave');
    expect(t.maxDays, 15);
    expect(t.isAttachmentRequired, isTrue);
  });

  test('LeaveBalance parses decimal fields', () {
    final b = LeaveBalance.fromJson({
      'leaveTypeId': 3,
      'leaveType': 'Sick leave',
      'entitlement': 15,
      'taken': 2.5,
      'remaining': 12.5,
    });
    expect(b.taken, 2.5);
    expect(b.remaining, 12.5);
  });

  test('LeaveRequest uppercases status and detects an attachment', () {
    final r = LeaveRequest.fromJson({
      'id': 7,
      'startDate': '2026-05-25T00:00:00',
      'endDate': '2026-05-25T00:00:00',
      'status': 'approved',
      'leaveName': 'Sick leave',
      'managerName': null,
      'note': '',
      'isHalfDay': false,
      'filePath': '/uploads/leaves/7-abc.pdf',
    });
    expect(r.status, 'APPROVED');
    expect(r.hasAttachment, isTrue);
    expect(r.startDate, DateTime(2026, 5, 25));
  });

  test('LeaveRequest with no attachment / null fields', () {
    final r = LeaveRequest.fromJson({'id': 1, 'status': 'PENDING'});
    expect(r.hasAttachment, isFalse);
    expect(r.managerName, '');
    expect(r.startDate, isNull);
  });
}

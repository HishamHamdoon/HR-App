// Leave domain models, decoded from Emp.Api.

/// A leave type (`LeaveTypesViewDto`). `GET /api/LeaveTypes` returns them all —
/// filter on [isActive] before offering them for a new request.
class LeaveType {
  const LeaveType({
    required this.id,
    required this.name,
    required this.note,
    required this.maxDays,
    required this.minDays,
    required this.isAttachmentRequired,
    required this.isActive,
  });

  final int id;
  final String name;
  final String note;
  final int maxDays;
  final int minDays;
  final bool isAttachmentRequired;
  final bool isActive;

  factory LeaveType.fromJson(Map<String, dynamic> j) => LeaveType(
    id: (j['id'] as num?)?.toInt() ?? 0,
    name: '${j['name'] ?? ''}',
    note: '${j['note'] ?? ''}',
    maxDays: (j['maxDays'] as num?)?.toInt() ?? 0,
    minDays: (j['minDays'] as num?)?.toInt() ?? 0,
    isAttachmentRequired: j['isAttachmentRequired'] == true,
    isActive: j['isActive'] == true,
  );
}

/// One row of the caller's annual balance (`GET /api/Leaves/balance/{id}`). The numbers
/// are decimals on the server (half-days), so they arrive as JSON numbers.
class LeaveBalance {
  const LeaveBalance({
    required this.leaveTypeId,
    required this.leaveType,
    required this.entitlement,
    required this.taken,
    required this.remaining,
  });

  final int leaveTypeId;
  final String leaveType;
  final double entitlement;
  final double taken;
  final double remaining;

  factory LeaveBalance.fromJson(Map<String, dynamic> j) => LeaveBalance(
    leaveTypeId: (j['leaveTypeId'] as num?)?.toInt() ?? 0,
    leaveType: '${j['leaveType'] ?? ''}',
    entitlement: (j['entitlement'] as num?)?.toDouble() ?? 0,
    taken: (j['taken'] as num?)?.toDouble() ?? 0,
    remaining: (j['remaining'] as num?)?.toDouble() ?? 0,
  );
}

/// A leave request in the caller's history (`ViewLeaveDto`).
class LeaveRequest {
  const LeaveRequest({
    required this.id,
    required this.startDate,
    required this.endDate,
    required this.status,
    required this.leaveName,
    required this.managerName,
    required this.note,
    required this.isHalfDay,
    required this.decisionNote,
    required this.filePath,
  });

  final int id;
  final DateTime? startDate;
  final DateTime? endDate;

  /// Canonical values are uppercase (`PENDING` / `APPROVED` / `REJECTED`).
  final String status;
  final String leaveName;
  final String managerName;
  final String note;
  final bool isHalfDay;
  final String decisionNote;

  /// Relative path (e.g. `/uploads/leaves/7-abc.pdf`) or empty. Read via the authorized
  /// `GET /api/Leaves/{id}/attachment` endpoint, never directly.
  final String filePath;

  bool get hasAttachment => filePath.isNotEmpty;

  factory LeaveRequest.fromJson(Map<String, dynamic> j) => LeaveRequest(
    id: (j['id'] as num?)?.toInt() ?? 0,
    startDate: _date(j['startDate']),
    endDate: _date(j['endDate']),
    status: '${j['status'] ?? ''}'.toUpperCase(),
    leaveName: '${j['leaveName'] ?? ''}',
    managerName: '${j['managerName'] ?? ''}',
    note: '${j['note'] ?? ''}',
    isHalfDay: j['isHalfDay'] == true,
    decisionNote: '${j['decisionNote'] ?? ''}',
    filePath: '${j['filePath'] ?? ''}',
  );

  static DateTime? _date(Object? v) =>
      (v is String && v.isNotEmpty) ? DateTime.tryParse(v) : null;
}

// A leave routed to the manager for a decision, from
// `GET /api/Leaves/get-leaves-by-managerId/{id}`. This projection is leaner than the
// employee's own history (ViewLeaveDto): no half-day, attachment, or manager-name fields.

class TeamLeave {
  const TeamLeave({
    required this.id,
    required this.startDate,
    required this.endDate,
    required this.status,
    required this.note,
    required this.employeeName,
    required this.leaveType,
  });

  final int id;
  final DateTime? startDate;
  final DateTime? endDate;

  /// Canonical uppercase (`PENDING` / `APPROVED` / `REJECTED`).
  final String status;
  final String note;
  final String employeeName;
  final String leaveType;

  bool get isPending => status == 'PENDING';

  factory TeamLeave.fromJson(Map<String, dynamic> j) => TeamLeave(
    id: (j['id'] as num?)?.toInt() ?? 0,
    startDate: _date(j['startDate']),
    endDate: _date(j['endDate']),
    status: '${j['status'] ?? ''}'.toUpperCase(),
    note: '${j['note'] ?? ''}',
    employeeName: '${j['employeeName'] ?? ''}',
    leaveType: '${j['leaveType'] ?? ''}',
  );

  static DateTime? _date(Object? v) =>
      (v is String && v.isNotEmpty) ? DateTime.tryParse(v) : null;
}

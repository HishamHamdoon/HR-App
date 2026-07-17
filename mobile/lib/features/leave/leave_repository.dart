import 'package:dio/dio.dart';

import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';
import 'leave_models.dart';

/// Leave reads and the create-request write against Emp.Api.
class LeaveRepository {
  LeaveRepository(this._api);

  final ApiClient _api;

  Future<List<LeaveType>> getLeaveTypes() async {
    final result = await _api.get('/api/LeaveTypes');
    return _list(result).map(LeaveType.fromJson).toList();
  }

  Future<List<LeaveBalance>> getBalance(int employeeId) async {
    final result = await _api.get('/api/Leaves/balance/$employeeId');
    return _list(result).map(LeaveBalance.fromJson).toList();
  }

  Future<List<LeaveRequest>> getMyLeaves(int employeeId) async {
    final result = await _api.get(
      '/api/Leaves/get-leaves-by-employeeId/$employeeId',
    );
    return _list(result).map(LeaveRequest.fromJson).toList();
  }

  /// Files a new request. `multipart/form-data` because the endpoint takes an optional
  /// `IFormFile Attachment`. Field names use the C# property names; ASP.NET form binding
  /// is case-insensitive. `ManagerId` is intentionally omitted — the server resolves the
  /// approver by walking the department tree and ignores any value sent here.
  Future<void> createLeave({
    required int employeeId,
    required int leaveTypeId,
    required DateTime startDate,
    required DateTime endDate,
    required bool isHalfDay,
    String? note,
    String? attachmentPath,
    String? attachmentName,
  }) async {
    final form = FormData.fromMap({
      'EmployeeId': '$employeeId',
      'LeavesTypeId': '$leaveTypeId',
      'StartDate': startDate.toIso8601String(),
      'EndDate': endDate.toIso8601String(),
      'IsHalfDay': isHalfDay ? 'true' : 'false',
      'Note': note ?? '',
      if (attachmentPath != null)
        'Attachment': await MultipartFile.fromFile(
          attachmentPath,
          filename: attachmentName,
        ),
    });
    await _api.post('/api/Leaves', body: form);
  }

  /// The envelope's `result` for these endpoints is a JSON array (or empty list).
  List<Map<String, dynamic>> _list(Object? result) {
    if (result is! List) {
      throw const ApiException('Unexpected list response.');
    }
    return result
        .whereType<Map>()
        .map((e) => Map<String, dynamic>.from(e))
        .toList();
  }
}

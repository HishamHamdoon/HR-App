import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';
import '../profile/employee_profile.dart';
import 'team_leave.dart';

/// Manager-side reads and the approve/reject write.
class ManagerRepository {
  ManagerRepository(this._api);

  final ApiClient _api;

  Future<List<TeamLeave>> getTeamLeaves(int managerId) async {
    final result = await _api.get(
      '/api/Leaves/get-leaves-by-managerId/$managerId',
    );
    return _list(result).map(TeamLeave.fromJson).toList();
  }

  /// The manager's team (their department plus sub-departments). Reuses EmployeeProfile
  /// since the endpoint returns the same EmployeeViewDto shape as a profile.
  Future<List<EmployeeProfile>> getTeam(int managerId) async {
    final result = await _api.get('/api/Employee/by-manager/$managerId');
    return _list(result).map(EmployeeProfile.fromJson).toList();
  }

  /// Approves or rejects a leave. `status` and `note` go on the query string, and the
  /// server requires exactly `Approved`/`Rejected` plus a note when rejecting.
  Future<void> decide({
    required int leaveId,
    required bool approved,
    String? note,
  }) async {
    await _api.patch(
      '/api/Leaves/$leaveId/decision',
      query: {
        'status': approved ? 'Approved' : 'Rejected',
        if (note != null && note.isNotEmpty) 'note': note,
      },
    );
  }

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

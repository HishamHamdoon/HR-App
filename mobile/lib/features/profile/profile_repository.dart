import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';
import 'employee_profile.dart';

/// Profile reads/writes against Emp.Api.
class ProfileRepository {
  ProfileRepository(this._api);

  final ApiClient _api;

  Future<EmployeeProfile> getById(int id) async {
    final result = await _api.get('/api/Employee/$id');
    if (result is! Map) {
      throw const ApiException('Unexpected profile response.');
    }
    return EmployeeProfile.fromJson(Map<String, dynamic>.from(result));
  }

  /// Self-service update. The server only applies phone and address (it ignores
  /// everything else on `UpdateProfileDto`) and derives the employee from the token.
  Future<void> updateMyProfile({
    required String phone,
    required String address,
  }) async {
    await _api.put(
      '/api/Employee/my-profile',
      body: {'phone': phone, 'address': address},
    );
  }
}

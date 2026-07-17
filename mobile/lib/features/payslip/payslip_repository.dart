import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';
import 'payslip_models.dart';

class PayslipRepository {
  PayslipRepository(this._api);

  final ApiClient _api;

  /// The caller's salary, or null when none is configured (the endpoint returns a null
  /// result with isSuccess=true in that case).
  Future<SalaryInfo?> getMySalary() async {
    final result = await _api.get('/api/Salary/mine');
    if (result == null) return null;
    if (result is! Map) {
      throw const ApiException('Unexpected salary response.');
    }
    return SalaryInfo.fromJson(Map<String, dynamic>.from(result));
  }

  Future<List<Payslip>> getMyPayrolls() async {
    final result = await _api.get('/api/Payrolls/mine');
    if (result is! List) {
      throw const ApiException('Unexpected payroll response.');
    }
    return result
        .whereType<Map>()
        .map((e) => Payslip.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }
}

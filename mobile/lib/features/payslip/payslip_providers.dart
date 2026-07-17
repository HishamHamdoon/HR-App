import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/providers.dart';
import 'payslip_models.dart';
import 'payslip_repository.dart';

final payslipRepositoryProvider = Provider<PayslipRepository>(
  (ref) => PayslipRepository(ref.watch(apiClientProvider)),
);

/// The caller's current salary (null when none is configured). The endpoint scopes to the
/// token, so this only depends on being logged in.
final mySalaryProvider = FutureProvider<SalaryInfo?>((ref) async {
  ref.watch(authControllerProvider);
  return ref.watch(payslipRepositoryProvider).getMySalary();
});

final myPayrollsProvider = FutureProvider<List<Payslip>>((ref) async {
  ref.watch(authControllerProvider);
  return ref.watch(payslipRepositoryProvider).getMyPayrolls();
});

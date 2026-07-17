import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import 'leave_models.dart';
import 'leave_repository.dart';

final leaveRepositoryProvider = Provider<LeaveRepository>(
  (ref) => LeaveRepository(ref.watch(apiClientProvider)),
);

int _employeeId(Ref ref) {
  final id = ref.watch(authControllerProvider).claims?.employeeId;
  if (id == null) {
    throw const ApiException('No employee id in the current session.');
  }
  return id;
}

/// Active leave types only — the set a new request may choose from.
final activeLeaveTypesProvider = FutureProvider<List<LeaveType>>((ref) async {
  final types = await ref.watch(leaveRepositoryProvider).getLeaveTypes();
  return types.where((t) => t.isActive).toList();
});

final myLeaveBalanceProvider = FutureProvider<List<LeaveBalance>>((ref) async {
  return ref.watch(leaveRepositoryProvider).getBalance(_employeeId(ref));
});

final myLeavesProvider = FutureProvider<List<LeaveRequest>>((ref) async {
  return ref.watch(leaveRepositoryProvider).getMyLeaves(_employeeId(ref));
});

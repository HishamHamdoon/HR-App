import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import '../profile/employee_profile.dart';
import 'manager_repository.dart';
import 'team_leave.dart';

final managerRepositoryProvider = Provider<ManagerRepository>(
  (ref) => ManagerRepository(ref.watch(apiClientProvider)),
);

int _managerId(Ref ref) {
  final id = ref.watch(authControllerProvider).claims?.employeeId;
  if (id == null) {
    throw const ApiException('No employee id in the current session.');
  }
  return id;
}

/// Leaves routed to the signed-in manager (their EmployeeId is the manager id).
final teamLeavesProvider = FutureProvider<List<TeamLeave>>((ref) async {
  return ref.watch(managerRepositoryProvider).getTeamLeaves(_managerId(ref));
});

final teamProvider = FutureProvider<List<EmployeeProfile>>((ref) async {
  return ref.watch(managerRepositoryProvider).getTeam(_managerId(ref));
});

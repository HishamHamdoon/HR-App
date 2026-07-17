import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import 'employee_profile.dart';
import 'profile_repository.dart';

final profileRepositoryProvider = Provider<ProfileRepository>(
  (ref) => ProfileRepository(ref.watch(apiClientProvider)),
);

/// The signed-in employee's own profile, keyed off the `EmployeeId` claim. `invalidate`
/// it after an edit to refetch.
final myProfileProvider = FutureProvider<EmployeeProfile>((ref) async {
  final id = ref.watch(authControllerProvider).claims?.employeeId;
  if (id == null) {
    throw const ApiException('No employee id in the current session.');
  }
  return ref.watch(profileRepositoryProvider).getById(id);
});

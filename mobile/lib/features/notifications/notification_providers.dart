import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/providers.dart';
import 'notification_models.dart';
import 'notification_repository.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>(
  (ref) => NotificationRepository(ref.watch(apiClientProvider)),
);

/// The caller's notification feed (recent items + unread count). Drives both the list
/// and the home badge; `invalidate` after mark-all-read to refresh both.
///
/// Without push (A6) this only refreshes on invalidation or a manual pull — there is no
/// live update yet.
final notificationsProvider = FutureProvider<NotificationFeed>((ref) async {
  // Re-fetch across a login/logout so a new session doesn't show the old feed.
  ref.watch(authControllerProvider);
  return ref.watch(notificationRepositoryProvider).getMine();
});

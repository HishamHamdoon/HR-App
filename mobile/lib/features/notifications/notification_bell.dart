import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../core/router.dart';
import '../../l10n/app_localizations.dart';
import 'notification_providers.dart';

/// App-bar bell that shows the unread count and opens the notifications screen. The count
/// comes from the same provider as the list, so marking-all-read updates both. Without
/// push (A6) it refreshes on navigation/invalidation, not live.
class NotificationBell extends ConsumerWidget {
  const NotificationBell({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final unread = ref.watch(
      notificationsProvider.select((v) => v.asData?.value.unreadCount ?? 0),
    );

    final button = IconButton(
      tooltip: l.notificationsTitle,
      icon: const Icon(Icons.notifications),
      onPressed: () => context.push(Routes.notifications),
    );

    if (unread == 0) return button;
    return Badge.count(count: unread, child: button);
  }
}

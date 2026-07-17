import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/api/api_envelope.dart';
import '../../core/router.dart';
import '../../l10n/app_localizations.dart';
import 'notification_models.dart';
import 'notification_providers.dart';

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final feed = ref.watch(notificationsProvider);
    final unread = feed.asData?.value.unreadCount ?? 0;

    return Scaffold(
      appBar: AppBar(
        title: Text(l.notificationsTitle),
        actions: [
          if (unread > 0)
            TextButton(
              onPressed: () async {
                final messenger = ScaffoldMessenger.of(context);
                try {
                  await ref.read(notificationRepositoryProvider).markAllRead();
                  ref.invalidate(notificationsProvider);
                } on ApiException catch (e) {
                  messenger.showSnackBar(SnackBar(content: Text(e.message)));
                }
              },
              child: Text(l.markAllRead),
            ),
        ],
      ),
      body: feed.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('$e', textAlign: TextAlign.center),
              const SizedBox(height: 12),
              OutlinedButton(
                onPressed: () => ref.invalidate(notificationsProvider),
                child: Text(l.retry),
              ),
            ],
          ),
        ),
        data: (data) {
          if (data.items.isEmpty) {
            return Center(child: Text(l.noNotifications));
          }
          return RefreshIndicator(
            onRefresh: () async => ref.invalidate(notificationsProvider),
            child: ListView.separated(
              itemCount: data.items.length,
              separatorBuilder: (_, _) => const Divider(height: 1),
              itemBuilder: (_, i) => _NotificationTile(item: data.items[i]),
            ),
          );
        },
      ),
    );
  }
}

class _NotificationTile extends StatelessWidget {
  const _NotificationTile({required this.item});

  final AppNotification item;

  @override
  Widget build(BuildContext context) {
    final scheme = Theme.of(context).colorScheme;
    final when = item.createdAt == null
        ? ''
        : DateFormat.yMMMd(
            Localizations.localeOf(context).toString(),
          ).add_jm().format(item.createdAt!);
    final target = _mapUrlToRoute(item.url);

    return ListTile(
      leading: Icon(
        item.isRead ? Icons.notifications_none : Icons.notifications_active,
        color: item.isRead ? scheme.outline : scheme.primary,
      ),
      title: Text(
        item.message,
        style: TextStyle(
          fontWeight: item.isRead ? FontWeight.normal : FontWeight.w600,
        ),
      ),
      subtitle: when.isEmpty ? null : Text(when),
      trailing: target != null ? const Icon(Icons.chevron_right) : null,
      onTap: target == null ? null : () => context.push(target),
    );
  }
}

/// Best-effort mapping of the server's stored web routes to mobile routes. The API stores
/// paths like `/Leaves/TeamLeaves` and `/Leaves/EmployeeLeaves`; anything unrecognised is
/// non-navigable rather than mapped wrongly. A6 (structured payloads) supersedes this.
String? _mapUrlToRoute(String url) {
  final u = url.toLowerCase();
  if (u.contains('teamleaves')) return Routes.teamLeaves;
  if (u.contains('employeeleaves') || u.contains('/leaves')) {
    return Routes.leave;
  }
  return null;
}

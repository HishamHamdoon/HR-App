// In-app notifications, from `GET /api/Notifications/mine`, whose result is an object
// `{ items: [...], unreadCount: N }` (not a bare array).

class AppNotification {
  const AppNotification({
    required this.id,
    required this.message,
    required this.url,
    required this.isRead,
    required this.createdAt,
  });

  final int id;
  final String message;

  /// A web route (e.g. `/Leaves/TeamLeaves`) — mapped best-effort to a mobile route on
  /// tap. Push (A6) will replace this with structured {type, entityId} fields.
  final String url;
  final bool isRead;
  final DateTime? createdAt;

  factory AppNotification.fromJson(Map<String, dynamic> j) => AppNotification(
    id: (j['id'] as num?)?.toInt() ?? 0,
    message: '${j['message'] ?? ''}',
    url: '${j['url'] ?? ''}',
    isRead: j['isRead'] == true,
    createdAt: switch (j['createdAt']) {
      final String s when s.isNotEmpty => DateTime.tryParse(s),
      _ => null,
    },
  );
}

class NotificationFeed {
  const NotificationFeed({required this.items, required this.unreadCount});

  final List<AppNotification> items;
  final int unreadCount;

  static const empty = NotificationFeed(items: [], unreadCount: 0);

  factory NotificationFeed.fromJson(Map<String, dynamic> j) {
    final rawItems = j['items'];
    return NotificationFeed(
      items: rawItems is List
          ? rawItems
                .whereType<Map>()
                .map(
                  (e) => AppNotification.fromJson(Map<String, dynamic>.from(e)),
                )
                .toList()
          : const [],
      unreadCount: (j['unreadCount'] as num?)?.toInt() ?? 0,
    );
  }
}

import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/notifications/notification_models.dart';

void main() {
  test('NotificationFeed parses the {items, unreadCount} object', () {
    final feed = NotificationFeed.fromJson({
      'items': [
        {
          'id': 1,
          'message': 'Sara requested Annual leave',
          'url': '/Leaves/TeamLeaves',
          'isRead': false,
          'createdAt': '2026-07-17T09:00:00',
        },
        {
          'id': 2,
          'message': 'Your leave was approved',
          'url': '/Leaves/EmployeeLeaves',
          'isRead': true,
          'createdAt': '2026-07-16T09:00:00',
        },
      ],
      'unreadCount': 1,
    });

    expect(feed.items.length, 2);
    expect(feed.unreadCount, 1);
    expect(feed.items.first.isRead, isFalse);
    expect(feed.items.first.createdAt, DateTime(2026, 7, 17, 9));
  });

  test('tolerates a missing items array', () {
    final feed = NotificationFeed.fromJson({'unreadCount': 0});
    expect(feed.items, isEmpty);
    expect(feed.unreadCount, 0);
  });

  test('empty constant', () {
    expect(NotificationFeed.empty.items, isEmpty);
    expect(NotificationFeed.empty.unreadCount, 0);
  });
}

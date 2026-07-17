import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/notifications/notification_models.dart';
import 'package:hr_app/features/notifications/notification_providers.dart';
import 'package:hr_app/features/notifications/notifications_screen.dart';
import 'package:hr_app/l10n/app_localizations.dart';

const _screen = MaterialApp(
  localizationsDelegates: [
    AppL10n.delegate,
    GlobalMaterialLocalizations.delegate,
    GlobalWidgetsLocalizations.delegate,
    GlobalCupertinoLocalizations.delegate,
  ],
  supportedLocales: AppL10n.supportedLocales,
  home: NotificationsScreen(),
);

void main() {
  testWidgets('lists notifications and offers mark-all-read when unread', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          notificationsProvider.overrideWith(
            (ref) async => NotificationFeed.fromJson({
              'items': [
                {
                  'id': 1,
                  'message': 'Sara requested Annual leave',
                  'url': '/Leaves/TeamLeaves',
                  'isRead': false,
                  'createdAt': '2026-07-17T09:00:00',
                },
              ],
              'unreadCount': 1,
            }),
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Sara requested Annual leave'), findsOneWidget);
    expect(find.text('Mark all read'), findsOneWidget);
  });

  testWidgets('empty state, no mark-all-read', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          notificationsProvider.overrideWith(
            (ref) async => NotificationFeed.empty,
          ),
        ],
        child: _screen,
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('You have no notifications.'), findsOneWidget);
    expect(find.text('Mark all read'), findsNothing);
  });
}

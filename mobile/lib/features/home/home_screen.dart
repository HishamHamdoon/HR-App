import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../core/providers.dart';
import '../../core/router.dart';
import '../../l10n/app_localizations.dart';

/// Landing screen after login. Still a placeholder body (the leave/team/notifications
/// shell lands in later phases), but now a real entry point to profile and password.
class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final claims = ref.watch(authControllerProvider).claims;

    return Scaffold(
      appBar: AppBar(
        title: Text(l.homeTitle),
        actions: [
          IconButton(
            tooltip: l.profileTitle,
            icon: const Icon(Icons.person),
            onPressed: () => context.push(Routes.profile),
          ),
          PopupMenuButton<String>(
            onSelected: (value) {
              switch (value) {
                case 'password':
                  context.push(Routes.changePassword);
                case 'signout':
                  ref.read(authControllerProvider).logout();
              }
            },
            itemBuilder: (_) => [
              PopupMenuItem(
                value: 'password',
                child: Text(l.changePasswordTitle),
              ),
              PopupMenuItem(value: 'signout', child: Text(l.signOut)),
            ],
          ),
        ],
      ),
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              l.welcome(claims?.name ?? ''),
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
            if (claims != null) ...[
              Text('EmployeeId: ${claims.employeeId ?? '-'}'),
              Text('Manager: ${claims.isManager}'),
              Text('Roles: ${claims.roles.join(', ')}'),
            ],
            const SizedBox(height: 24),
            FilledButton.icon(
              onPressed: () => context.push(Routes.leave),
              icon: const Icon(Icons.event_available),
              label: Text(l.leaveTitle),
            ),
            const SizedBox(height: 12),
            FilledButton.tonalIcon(
              onPressed: () => context.push(Routes.profile),
              icon: const Icon(Icons.person),
              label: Text(l.profileTitle),
            ),
          ],
        ),
      ),
    );
  }
}

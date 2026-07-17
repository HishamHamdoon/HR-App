import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/providers.dart';
import '../../l10n/app_localizations.dart';

/// Landing screen after login. A placeholder that proves the session is live by showing
/// the decoded claims; Phase 3+ replaces the body with the real navigation shell (leave,
/// team, notifications, payslips).
class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final auth = ref.watch(authControllerProvider);
    final claims = auth.claims;

    return Scaffold(
      appBar: AppBar(
        title: Text(l.homeTitle),
        actions: [
          IconButton(
            tooltip: l.signOut,
            icon: const Icon(Icons.logout),
            onPressed: () => ref.read(authControllerProvider).logout(),
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
          ],
        ),
      ),
    );
  }
}

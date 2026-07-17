import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../l10n/app_localizations.dart';
import 'manager_providers.dart';

/// The manager's team roster (department + sub-departments), read-only.
class MyTeamScreen extends ConsumerWidget {
  const MyTeamScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final team = ref.watch(teamProvider);

    return Scaffold(
      appBar: AppBar(title: Text(l.myTeamTitle)),
      body: team.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('$e', textAlign: TextAlign.center),
              const SizedBox(height: 12),
              OutlinedButton(
                onPressed: () => ref.invalidate(teamProvider),
                child: Text(l.retry),
              ),
            ],
          ),
        ),
        data: (members) {
          if (members.isEmpty) {
            return Center(child: Text(l.noTeamMembers));
          }
          return RefreshIndicator(
            onRefresh: () async => ref.invalidate(teamProvider),
            child: ListView.separated(
              itemCount: members.length,
              separatorBuilder: (_, _) => const Divider(height: 1),
              itemBuilder: (_, i) {
                final m = members[i];
                return ListTile(
                  leading: CircleAvatar(
                    child: Text(
                      m.name.isNotEmpty ? m.name[0].toUpperCase() : '?',
                    ),
                  ),
                  title: Text(m.name),
                  subtitle: Text(
                    [
                      m.jobTitle,
                      m.phone,
                    ].where((s) => s.isNotEmpty).join(' · '),
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }
}

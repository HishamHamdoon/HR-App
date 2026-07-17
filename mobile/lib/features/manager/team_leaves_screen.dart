import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/api/api_envelope.dart';
import '../../l10n/app_localizations.dart';
import 'manager_providers.dart';
import 'team_leave.dart';

/// The manager's approval queue. Two tabs: leaves still pending a decision, and the full
/// history of everything routed to them.
class TeamLeavesScreen extends ConsumerWidget {
  const TeamLeavesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Text(l.teamLeavesTitle),
          bottom: TabBar(
            tabs: [
              Tab(text: l.pendingTab),
              Tab(text: l.allTab),
            ],
          ),
        ),
        body: const TabBarView(
          children: [
            _TeamLeaveList(pendingOnly: true),
            _TeamLeaveList(pendingOnly: false),
          ],
        ),
      ),
    );
  }
}

class _TeamLeaveList extends ConsumerWidget {
  const _TeamLeaveList({required this.pendingOnly});

  final bool pendingOnly;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final leaves = ref.watch(teamLeavesProvider);

    return leaves.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (e, _) => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('$e', textAlign: TextAlign.center),
            const SizedBox(height: 12),
            OutlinedButton(
              onPressed: () => ref.invalidate(teamLeavesProvider),
              child: Text(l.retry),
            ),
          ],
        ),
      ),
      data: (all) {
        final items =
            (pendingOnly ? all.where((x) => x.isPending) : all).toList()..sort(
              (a, b) => (b.startDate ?? DateTime(0)).compareTo(
                a.startDate ?? DateTime(0),
              ),
            );
        if (items.isEmpty) {
          return Center(
            child: Text(pendingOnly ? l.noPendingLeaves : l.noTeamLeaves),
          );
        }
        return RefreshIndicator(
          onRefresh: () async => ref.invalidate(teamLeavesProvider),
          child: ListView.separated(
            padding: const EdgeInsets.all(12),
            itemCount: items.length,
            separatorBuilder: (_, _) => const SizedBox(height: 8),
            itemBuilder: (_, i) => _TeamLeaveCard(leave: items[i]),
          ),
        );
      },
    );
  }
}

class _TeamLeaveCard extends ConsumerStatefulWidget {
  const _TeamLeaveCard({required this.leave});

  final TeamLeave leave;

  @override
  ConsumerState<_TeamLeaveCard> createState() => _TeamLeaveCardState();
}

class _TeamLeaveCardState extends ConsumerState<_TeamLeaveCard> {
  bool _busy = false;

  Future<void> _decide({required bool approved, String? note}) async {
    final l = AppL10n.of(context);
    final messenger = ScaffoldMessenger.of(context);
    setState(() => _busy = true);
    try {
      await ref
          .read(managerRepositoryProvider)
          .decide(leaveId: widget.leave.id, approved: approved, note: note);
      ref.invalidate(teamLeavesProvider);
      messenger.showSnackBar(
        SnackBar(content: Text(approved ? l.leaveApproved : l.leaveRejected)),
      );
    } on ApiException catch (e) {
      messenger.showSnackBar(SnackBar(content: Text(e.message)));
      if (mounted) setState(() => _busy = false);
    }
    // On success the card is rebuilt from the refreshed list, so no need to reset _busy.
  }

  Future<void> _approve() async {
    final l = AppL10n.of(context);
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        content: Text(
          l.approveConfirm(widget.leave.employeeName, widget.leave.leaveType),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text(l.cancel),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(l.approve),
          ),
        ],
      ),
    );
    if (ok == true) await _decide(approved: true);
  }

  Future<void> _reject() async {
    final l = AppL10n.of(context);
    final controller = TextEditingController();
    final reason = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(l.reject),
        content: TextField(
          controller: controller,
          autofocus: true,
          decoration: InputDecoration(labelText: l.rejectReason),
          maxLines: 2,
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: Text(l.cancel),
          ),
          FilledButton(
            onPressed: () {
              if (controller.text.trim().isEmpty) return;
              Navigator.pop(ctx, controller.text.trim());
            },
            child: Text(l.reject),
          ),
        ],
      ),
    );
    controller.dispose();
    if (reason != null && reason.isNotEmpty) {
      await _decide(approved: false, note: reason);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final leave = widget.leave;
    final df = DateFormat.yMMMd(Localizations.localeOf(context).toString());
    final range = leave.startDate == null
        ? ''
        : leave.endDate == null || leave.endDate == leave.startDate
        ? df.format(leave.startDate!)
        : '${df.format(leave.startDate!)} – ${df.format(leave.endDate!)}';

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              leave.employeeName,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 4),
            Text('${leave.leaveType} · $range'),
            if (leave.note.isNotEmpty) ...[
              const SizedBox(height: 4),
              Text(leave.note, style: Theme.of(context).textTheme.bodySmall),
            ],
            if (leave.isPending) ...[
              const SizedBox(height: 12),
              if (_busy)
                const Center(
                  child: Padding(
                    padding: EdgeInsets.all(8),
                    child: SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    ),
                  ),
                )
              else
                Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    OutlinedButton(onPressed: _reject, child: Text(l.reject)),
                    const SizedBox(width: 8),
                    FilledButton(onPressed: _approve, child: Text(l.approve)),
                  ],
                ),
            ] else ...[
              const SizedBox(height: 8),
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: Chip(
                  label: Text(
                    leave.status == 'APPROVED'
                        ? l.statusApproved
                        : l.statusRejected,
                    style: const TextStyle(fontSize: 12),
                  ),
                  visualDensity: VisualDensity.compact,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

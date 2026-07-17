import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../l10n/app_localizations.dart';
import 'leave_models.dart';
import 'leave_providers.dart';
import 'leave_request_screen.dart';

/// Two tabs: the caller's annual balance per leave type, and their request history.
class LeaveScreen extends ConsumerWidget {
  const LeaveScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Text(l.leaveTitle),
          bottom: TabBar(
            tabs: [
              Tab(text: l.leaveBalanceTab),
              Tab(text: l.leaveHistoryTab),
            ],
          ),
        ),
        body: const TabBarView(children: [_BalanceTab(), _HistoryTab()]),
        floatingActionButton: FloatingActionButton.extended(
          onPressed: () async {
            final created = await Navigator.of(context).push<bool>(
              MaterialPageRoute(builder: (_) => const LeaveRequestScreen()),
            );
            if (created == true) {
              ref.invalidate(myLeavesProvider);
              ref.invalidate(myLeaveBalanceProvider);
            }
          },
          icon: const Icon(Icons.add),
          label: Text(l.requestLeave),
        ),
      ),
    );
  }
}

class _BalanceTab extends ConsumerWidget {
  const _BalanceTab();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final balances = ref.watch(myLeaveBalanceProvider);
    return balances.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (e, _) => _Error(
        message: '$e',
        onRetry: () => ref.invalidate(myLeaveBalanceProvider),
      ),
      data: (rows) {
        if (rows.isEmpty) return Center(child: Text(l.noBalances));
        return RefreshIndicator(
          onRefresh: () async => ref.invalidate(myLeaveBalanceProvider),
          child: ListView.separated(
            padding: const EdgeInsets.all(12),
            itemCount: rows.length,
            separatorBuilder: (_, _) => const SizedBox(height: 8),
            itemBuilder: (_, i) => _BalanceCard(row: rows[i]),
          ),
        );
      },
    );
  }
}

class _BalanceCard extends StatelessWidget {
  const _BalanceCard({required this.row});

  final LeaveBalance row;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final fraction = row.entitlement > 0
        ? (row.taken / row.entitlement).clamp(0.0, 1.0)
        : 0.0;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    row.leaveType,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                Text(
                  l.remainingDays(_n(row.remaining)),
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: Theme.of(context).colorScheme.primary,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            LinearProgressIndicator(value: fraction),
            const SizedBox(height: 8),
            Text(
              l.entitlementTakenRemaining(
                _n(row.entitlement),
                _n(row.taken),
                _n(row.remaining),
              ),
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }
}

class _HistoryTab extends ConsumerWidget {
  const _HistoryTab();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final leaves = ref.watch(myLeavesProvider);
    return leaves.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (e, _) => _Error(
        message: '$e',
        onRetry: () => ref.invalidate(myLeavesProvider),
      ),
      data: (items) {
        if (items.isEmpty) return Center(child: Text(l.noLeaves));
        final sorted = [...items]
          ..sort(
            (a, b) => (b.startDate ?? DateTime(0)).compareTo(
              a.startDate ?? DateTime(0),
            ),
          );
        return RefreshIndicator(
          onRefresh: () async => ref.invalidate(myLeavesProvider),
          child: ListView.separated(
            padding: const EdgeInsets.all(12),
            itemCount: sorted.length,
            separatorBuilder: (_, _) => const SizedBox(height: 8),
            itemBuilder: (_, i) => _LeaveCard(leave: sorted[i]),
          ),
        );
      },
    );
  }
}

class _LeaveCard extends StatelessWidget {
  const _LeaveCard({required this.leave});

  final LeaveRequest leave;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final df = DateFormat.yMMMd(Localizations.localeOf(context).toString());
    final range = leave.startDate == null
        ? ''
        : leave.endDate == null || leave.endDate == leave.startDate
        ? df.format(leave.startDate!)
        : '${df.format(leave.startDate!)} – ${df.format(leave.endDate!)}';

    return Card(
      child: ListTile(
        title: Text(leave.leaveName),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(range),
            if (leave.isHalfDay) Text(l.halfDay),
            if (leave.note.isNotEmpty) Text(leave.note),
          ],
        ),
        isThreeLine: leave.note.isNotEmpty,
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            _StatusChip(status: leave.status),
            if (leave.hasAttachment)
              const Padding(
                padding: EdgeInsets.only(top: 4),
                child: Icon(Icons.attachment, size: 18),
              ),
          ],
        ),
      ),
    );
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final scheme = Theme.of(context).colorScheme;
    final (label, color) = switch (status) {
      'APPROVED' => (l.statusApproved, Colors.green),
      'REJECTED' => (l.statusRejected, scheme.error),
      _ => (l.statusPending, Colors.orange),
    };
    return Chip(
      label: Text(label, style: const TextStyle(fontSize: 12)),
      backgroundColor: color.withValues(alpha: 0.15),
      side: BorderSide(color: color),
      visualDensity: VisualDensity.compact,
    );
  }
}

class _Error extends StatelessWidget {
  const _Error({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: onRetry, child: Text(l.retry)),
          ],
        ),
      ),
    );
  }
}

/// Formats a leave count without a trailing `.0` (2.5 stays 2.5, 3.0 becomes 3).
String _n(double v) => v == v.roundToDouble() ? v.toInt().toString() : '$v';

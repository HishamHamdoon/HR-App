import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../l10n/app_localizations.dart';
import 'payslip_models.dart';
import 'payslip_providers.dart';

/// The caller's current salary breakdown plus their payroll history. Both come from the
/// A4 self-service endpoints; either may be empty.
class PayslipScreen extends ConsumerWidget {
  const PayslipScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final salary = ref.watch(mySalaryProvider);
    final payrolls = ref.watch(myPayrollsProvider);

    return Scaffold(
      appBar: AppBar(title: Text(l.payslipTitle)),
      body: RefreshIndicator(
        onRefresh: () async {
          ref.invalidate(mySalaryProvider);
          ref.invalidate(myPayrollsProvider);
        },
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Text(
              l.currentSalary,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            salary.when(
              loading: () => const Padding(
                padding: EdgeInsets.all(24),
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (e, _) => _InlineError(
                message: '$e',
                onRetry: () => ref.invalidate(mySalaryProvider),
              ),
              data: (s) => s == null
                  ? _EmptyCard(text: l.noSalaryConfigured)
                  : _SalaryCard(salary: s),
            ),
            const SizedBox(height: 24),
            Text(
              l.payrollHistory,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            payrolls.when(
              loading: () => const Padding(
                padding: EdgeInsets.all(24),
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (e, _) => _InlineError(
                message: '$e',
                onRetry: () => ref.invalidate(myPayrollsProvider),
              ),
              data: (list) => list.isEmpty
                  ? _EmptyCard(text: l.noPayrolls)
                  : Column(
                      children: [
                        for (final p in list) _PayslipCard(payslip: p),
                      ],
                    ),
            ),
          ],
        ),
      ),
    );
  }
}

class _SalaryCard extends StatelessWidget {
  const _SalaryCard({required this.salary});

  final SalaryInfo salary;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final money = NumberFormat.decimalPattern(
      Localizations.localeOf(context).toString(),
    );
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            _MoneyRow(
              label: l.basicSalary,
              value: money.format(salary.basicSalary),
            ),
            _MoneyRow(
              label: l.allowances,
              value: money.format(salary.allowances),
            ),
            _MoneyRow(
              label: l.deductions,
              value: '- ${money.format(salary.deductions)}',
            ),
            const Divider(),
            _MoneyRow(
              label: l.netSalary,
              value: money.format(salary.netSalary),
              emphasize: true,
            ),
            if (salary.effectiveDate != null) ...[
              const SizedBox(height: 8),
              Align(
                alignment: AlignmentDirectional.centerStart,
                child: Text(
                  l.effectiveFrom(
                    DateFormat.yMMMd(
                      Localizations.localeOf(context).toString(),
                    ).format(salary.effectiveDate!),
                  ),
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _PayslipCard extends StatelessWidget {
  const _PayslipCard({required this.payslip});

  final Payslip payslip;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final locale = Localizations.localeOf(context).toString();
    final money = NumberFormat.decimalPattern(locale);
    final month = payslip.salaryMonth == null
        ? ''
        : DateFormat.yMMMM(locale).format(payslip.salaryMonth!);
    final scheme = Theme.of(context).colorScheme;

    return Card(
      child: ListTile(
        title: Text(month),
        subtitle: Text(
          '${l.grossSalary} ${money.format(payslip.grossSalary)} · '
          '${l.netSalary} ${money.format(payslip.netSalary)}',
        ),
        trailing: Chip(
          label: Text(
            payslip.isPaid ? l.paid : l.unpaid,
            style: const TextStyle(fontSize: 12),
          ),
          backgroundColor: (payslip.isPaid ? Colors.green : scheme.error)
              .withValues(alpha: 0.15),
          visualDensity: VisualDensity.compact,
        ),
      ),
    );
  }
}

class _MoneyRow extends StatelessWidget {
  const _MoneyRow({
    required this.label,
    required this.value,
    this.emphasize = false,
  });

  final String label;
  final String value;
  final bool emphasize;

  @override
  Widget build(BuildContext context) {
    final style = emphasize
        ? Theme.of(context).textTheme.titleMedium
        : Theme.of(context).textTheme.bodyLarge;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: style),
          Text(value, style: style),
        ],
      ),
    );
  }
}

class _EmptyCard extends StatelessWidget {
  const _EmptyCard({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Center(child: Text(text, textAlign: TextAlign.center)),
      ),
    );
  }
}

class _InlineError extends StatelessWidget {
  const _InlineError({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 8),
            OutlinedButton(onPressed: onRetry, child: Text(l.retry)),
          ],
        ),
      ),
    );
  }
}

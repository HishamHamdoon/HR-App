import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import '../../l10n/app_localizations.dart';
import 'leave_calc.dart';
import 'leave_models.dart';
import 'leave_providers.dart';

/// Form to file a new leave request. Client-side checks mirror the server's rules so the
/// user gets instant feedback; the server remains the authority (overlap and annual
/// balance are only enforced there, and surface as an ApiException message).
class LeaveRequestScreen extends ConsumerStatefulWidget {
  const LeaveRequestScreen({super.key});

  @override
  ConsumerState<LeaveRequestScreen> createState() => _LeaveRequestScreenState();
}

class _LeaveRequestScreenState extends ConsumerState<LeaveRequestScreen> {
  LeaveType? _type;
  DateTime? _start;
  DateTime? _end;
  bool _halfDay = false;
  final _note = TextEditingController();
  PlatformFile? _attachment;
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _note.dispose();
    super.dispose();
  }

  Future<void> _pickDate({required bool isStart}) async {
    final now = DateTime.now();
    final initial = (isStart ? _start : _end) ?? _start ?? now;
    final picked = await showDatePicker(
      context: context,
      initialDate: initial,
      firstDate: DateTime(now.year - 1),
      lastDate: DateTime(now.year + 2),
    );
    if (picked == null) return;
    setState(() {
      if (isStart) {
        _start = picked;
        if (_end != null && _end!.isBefore(picked)) _end = picked;
      } else {
        _end = picked;
      }
    });
  }

  Future<void> _pickAttachment() async {
    final result = await FilePicker.platform.pickFiles(withReadStream: false);
    if (result != null && result.files.isNotEmpty) {
      setState(() => _attachment = result.files.first);
    }
  }

  /// Returns a validation message, or null if the form is ready to submit.
  String? _validate(AppL10n l) {
    final type = _type;
    final start = _start;
    final end = _end;
    if (type == null || start == null || end == null) return l.fieldRequired;
    if (end.isBefore(start)) return l.endBeforeStart;
    if (_halfDay && !_sameDay(start, end)) return l.halfDaySameDay;

    final days = effectiveDays(start, end, _halfDay);
    final shown = _n(days);
    if (type.minDays > 0 && days < type.minDays) {
      return l.minDaysRequired(type.minDays, shown);
    }
    if (type.maxDays > 0 && days > type.maxDays) {
      return l.maxDaysAllowed(type.maxDays, shown);
    }
    if (type.isAttachmentRequired && _attachment == null) {
      return l.attachmentRequired;
    }
    return null;
  }

  Future<void> _submit() async {
    final l = AppL10n.of(context);
    final problem = _validate(l);
    if (problem != null) {
      setState(() => _error = problem);
      return;
    }
    final employeeId = ref.read(authControllerProvider).claims?.employeeId;
    if (employeeId == null) {
      setState(() => _error = 'No employee id in session.');
      return;
    }
    final navigator = Navigator.of(context);
    final messenger = ScaffoldMessenger.of(context);

    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await ref
          .read(leaveRepositoryProvider)
          .createLeave(
            employeeId: employeeId,
            leaveTypeId: _type!.id,
            startDate: _start!,
            endDate: _end!,
            isHalfDay: _halfDay,
            note: _note.text.trim(),
            attachmentPath: _attachment?.path,
            attachmentName: _attachment?.name,
          );
      messenger.showSnackBar(SnackBar(content: Text(l.leaveSubmitted)));
      navigator.pop(true);
    } on ApiException catch (e) {
      setState(() => _error = e.message);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final df = DateFormat.yMMMd(Localizations.localeOf(context).toString());
    final types = ref.watch(activeLeaveTypesProvider);

    return Scaffold(
      appBar: AppBar(title: Text(l.requestLeave)),
      body: types.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('$e')),
        data: (list) => ListView(
          padding: const EdgeInsets.all(16),
          children: [
            DropdownButtonFormField<LeaveType>(
              initialValue: _type,
              decoration: InputDecoration(labelText: l.leaveType),
              items: [
                for (final t in list)
                  DropdownMenuItem(value: t, child: Text(t.name)),
              ],
              onChanged: (t) => setState(() {
                _type = t;
                _error = null;
              }),
            ),
            const SizedBox(height: 16),
            _DateField(
              label: l.startDate,
              value: _start == null ? null : df.format(_start!),
              onTap: () => _pickDate(isStart: true),
            ),
            const SizedBox(height: 12),
            _DateField(
              label: l.endDate,
              value: _end == null ? null : df.format(_end!),
              onTap: () => _pickDate(isStart: false),
            ),
            SwitchListTile(
              contentPadding: EdgeInsets.zero,
              title: Text(l.halfDay),
              value: _halfDay,
              onChanged: (v) => setState(() => _halfDay = v),
            ),
            TextField(
              controller: _note,
              decoration: InputDecoration(labelText: l.note),
              maxLines: 2,
            ),
            const SizedBox(height: 16),
            OutlinedButton.icon(
              onPressed: _pickAttachment,
              icon: const Icon(Icons.attach_file),
              label: Text(_attachment?.name ?? l.addAttachment),
            ),
            if (_type?.isAttachmentRequired ?? false)
              Padding(
                padding: const EdgeInsets.only(top: 4),
                child: Text(
                  l.attachmentRequired,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ),
            if (_error != null) ...[
              const SizedBox(height: 16),
              Text(
                _error!,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ],
            const SizedBox(height: 24),
            FilledButton(
              onPressed: _busy ? null : _submit,
              child: _busy
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(l.submit),
            ),
          ],
        ),
      ),
    );
  }
}

class _DateField extends StatelessWidget {
  const _DateField({
    required this.label,
    required this.value,
    required this.onTap,
  });

  final String label;
  final String? value;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: label,
          suffixIcon: const Icon(Icons.calendar_today),
        ),
        child: Text(value ?? ''),
      ),
    );
  }
}

bool _sameDay(DateTime a, DateTime b) =>
    a.year == b.year && a.month == b.month && a.day == b.day;

String _n(double v) => v == v.roundToDouble() ? v.toInt().toString() : '$v';

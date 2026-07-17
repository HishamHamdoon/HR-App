import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../l10n/app_localizations.dart';
import 'employee_profile.dart';
import 'profile_providers.dart';

/// Shows the signed-in employee's profile and lets them edit the two self-service fields
/// (phone, address). Everything else is read-only.
class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    final profile = ref.watch(myProfileProvider);

    return Scaffold(
      appBar: AppBar(title: Text(l.profileTitle)),
      body: profile.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => _ErrorView(
          message: '$e',
          onRetry: () => ref.invalidate(myProfileProvider),
        ),
        data: (p) => _ProfileView(profile: p),
      ),
      floatingActionButton: profile.hasValue
          ? FloatingActionButton.extended(
              onPressed: () => _openEdit(context, ref, profile.requireValue),
              icon: const Icon(Icons.edit),
              label: Text(l.editProfile),
            )
          : null,
    );
  }

  Future<void> _openEdit(
    BuildContext context,
    WidgetRef ref,
    EmployeeProfile p,
  ) async {
    final changed = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      builder: (_) => _EditProfileSheet(profile: p),
    );
    if (changed == true) ref.invalidate(myProfileProvider);
  }
}

class _ProfileView extends StatelessWidget {
  const _ProfileView({required this.profile});

  final EmployeeProfile profile;

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final df = DateFormat.yMMMMd(Localizations.localeOf(context).toString());
    String date(DateTime? d) => d == null ? l.notProvided : df.format(d);
    String orDash(String s) => s.isEmpty ? l.notProvided : s;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Center(
          child: Column(
            children: [
              CircleAvatar(
                radius: 36,
                child: Text(
                  profile.name.isNotEmpty ? profile.name[0].toUpperCase() : '?',
                  style: const TextStyle(fontSize: 28),
                ),
              ),
              const SizedBox(height: 12),
              Text(
                profile.name,
                style: Theme.of(context).textTheme.headlineSmall,
                textAlign: TextAlign.center,
              ),
              Text(orDash(profile.jobTitle)),
            ],
          ),
        ),
        const SizedBox(height: 24),
        _Row(label: l.email, value: orDash(profile.email)),
        _Row(label: l.phone, value: orDash(profile.phone)),
        _Row(label: l.address, value: orDash(profile.address)),
        const Divider(height: 32),
        _Row(label: l.department, value: orDash(profile.departmentName)),
        _Row(label: l.jobTitle, value: orDash(profile.jobTitle)),
        _Row(label: l.manager, value: orDash(profile.manager)),
        _Row(label: l.country, value: orDash(profile.country)),
        const Divider(height: 32),
        _Row(label: l.birthDate, value: date(profile.birthDate)),
        _Row(label: l.hireDate, value: date(profile.hireDate)),
      ],
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(
              label,
              style: TextStyle(color: Theme.of(context).colorScheme.outline),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message, required this.onRetry});

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
            Icon(
              Icons.error_outline,
              color: Theme.of(context).colorScheme.error,
              size: 40,
            ),
            const SizedBox(height: 12),
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: onRetry, child: Text(l.retry)),
          ],
        ),
      ),
    );
  }
}

class _EditProfileSheet extends ConsumerStatefulWidget {
  const _EditProfileSheet({required this.profile});

  final EmployeeProfile profile;

  @override
  ConsumerState<_EditProfileSheet> createState() => _EditProfileSheetState();
}

class _EditProfileSheetState extends ConsumerState<_EditProfileSheet> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _phone = TextEditingController(
    text: widget.profile.phone,
  );
  late final TextEditingController _address = TextEditingController(
    text: widget.profile.address,
  );
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _phone.dispose();
    _address.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.validate() ?? false)) return;
    FocusScope.of(context).unfocus();
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await ref
          .read(profileRepositoryProvider)
          .updateMyProfile(
            phone: _phone.text.trim(),
            address: _address.text.trim(),
          );
      if (mounted) Navigator.of(context).pop(true);
    } on Object catch (e) {
      setState(() => _error = '$e');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    return Padding(
      // Lift above the keyboard.
      padding: EdgeInsets.fromLTRB(
        16,
        16,
        16,
        16 + MediaQuery.of(context).viewInsets.bottom,
      ),
      child: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(l.editProfile, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 16),
            TextFormField(
              controller: _phone,
              decoration: InputDecoration(labelText: l.phone),
              keyboardType: TextInputType.phone,
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? l.fieldRequired : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _address,
              decoration: InputDecoration(labelText: l.address),
              maxLines: 2,
            ),
            if (_error != null) ...[
              const SizedBox(height: 12),
              Text(
                _error!,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ],
            const SizedBox(height: 20),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: _busy ? null : () => Navigator.of(context).pop(),
                    child: Text(l.cancel),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: FilledButton(
                    onPressed: _busy ? null : _save,
                    child: _busy
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : Text(l.save),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

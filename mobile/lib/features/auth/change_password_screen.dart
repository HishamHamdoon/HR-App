import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import '../../l10n/app_localizations.dart';
import 'auth_providers.dart';

/// Change password. Reached two ways: the router pins here when the token carries
/// `MustChangePassword=true` (forced first-login), or the user navigates here voluntarily
/// later. Because the API returns no new token, a forced change ends with a sign-out so
/// the next login issues a clean token — otherwise the stale claim would trap the user.
class ChangePasswordScreen extends ConsumerStatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  ConsumerState<ChangePasswordScreen> createState() =>
      _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends ConsumerState<ChangePasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  final _current = TextEditingController();
  final _next = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _current.dispose();
    _next.dispose();
    _confirm.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) return;
    FocusScope.of(context).unfocus();
    final auth = ref.read(authControllerProvider);
    final forced = auth.mustChangePassword;
    final l = AppL10n.of(context);
    final messenger = ScaffoldMessenger.of(context);
    final navigator = Navigator.of(context);

    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await ref
          .read(authRepositoryProvider)
          .changePassword(_current.text, _next.text);

      if (forced) {
        // Stored token still says MustChangePassword; only a fresh login clears it.
        await auth.logoutWithNotice(l.passwordChangedSignInAgain);
      } else {
        messenger.showSnackBar(SnackBar(content: Text(l.profileUpdated)));
        navigator.maybePop();
      }
    } on ApiException catch (e) {
      setState(() => _error = e.message);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    final forced = ref.watch(authControllerProvider).mustChangePassword;

    return Scaffold(
      appBar: AppBar(
        title: Text(l.changePasswordTitle),
        // No back arrow when forced — there is nowhere else to go until it is done.
        automaticallyImplyLeading: !forced,
      ),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 420),
          child: Form(
            key: _formKey,
            child: ListView(
              shrinkWrap: true,
              padding: const EdgeInsets.all(24),
              children: [
                if (forced) ...[
                  Card(
                    color: Theme.of(context).colorScheme.secondaryContainer,
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Text(l.mustChangePasswordNotice),
                    ),
                  ),
                  const SizedBox(height: 16),
                ],
                TextFormField(
                  controller: _current,
                  decoration: InputDecoration(labelText: l.currentPassword),
                  obscureText: true,
                  validator: (v) =>
                      (v == null || v.isEmpty) ? l.fieldRequired : null,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _next,
                  decoration: InputDecoration(labelText: l.newPassword),
                  obscureText: true,
                  validator: (v) =>
                      (v == null || v.isEmpty) ? l.fieldRequired : null,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _confirm,
                  decoration: InputDecoration(labelText: l.confirmPassword),
                  obscureText: true,
                  onFieldSubmitted: (_) => _submit(),
                  validator: (v) =>
                      v != _next.text ? l.passwordsDoNotMatch : null,
                ),
                if (_error != null) ...[
                  const SizedBox(height: 16),
                  Text(
                    _error!,
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.error,
                    ),
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
                      : Text(l.save),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

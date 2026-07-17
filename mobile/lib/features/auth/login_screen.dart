import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_envelope.dart';
import '../../core/providers.dart';
import '../../l10n/app_localizations.dart';

/// Minimal working login: proves the envelope, interceptor, and token store end to end.
/// Phase 3 replaces this with the real form (validation, remember-me, error surfaces).
class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _username = TextEditingController();
  final _password = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _username.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final result = await ref
          .read(apiClientProvider)
          .post(
            '/api/Auth/login',
            body: {
              'username': _username.text.trim(),
              'password': _password.text,
            },
          );
      final token = (result is Map) ? result['token'] as String? : null;
      if (token == null || token.isEmpty) {
        throw const ApiException('No token returned.');
      }
      await ref.read(authControllerProvider).onLoggedIn(token);
      // The router's redirect takes it from here.
    } on ApiException catch (e) {
      setState(() => _error = e.message);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l = AppL10n.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.loginTitle)),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 420),
          child: ListView(
            shrinkWrap: true,
            padding: const EdgeInsets.all(24),
            children: [
              TextField(
                controller: _username,
                decoration: InputDecoration(labelText: l.username),
                textInputAction: TextInputAction.next,
                autofillHints: const [AutofillHints.username],
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _password,
                decoration: InputDecoration(labelText: l.password),
                obscureText: true,
                onSubmitted: (_) => _submit(),
                autofillHints: const [AutofillHints.password],
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
                    : Text(l.signIn),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

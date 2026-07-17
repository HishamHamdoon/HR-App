import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../l10n/app_localizations.dart';

/// Placeholder. The router already pins here when the token carries
/// `MustChangePassword=true`; Phase 3 wires the POST /api/Auth/change-password call and,
/// on success, re-logs-in with the returned token to clear the flag.
class ChangePasswordScreen extends ConsumerWidget {
  const ChangePasswordScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l = AppL10n.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l.changePasswordTitle)),
      body: Center(child: Text(l.changePasswordTitle)),
    );
  }
}

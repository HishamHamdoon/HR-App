import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'api/api_client.dart';
import 'auth/auth_controller.dart';
import 'auth/token_store.dart';

/// Wiring for the core singletons. Feature providers layer on top of these.

final tokenStoreProvider = Provider<TokenStore>((ref) => TokenStore());

/// AuthController is a ChangeNotifier (go_router listens to it directly). Riverpod 3
/// dropped ChangeNotifierProvider from the core API, so expose the instance through a
/// plain Provider and dispose it with the container.
final authControllerProvider = Provider<AuthController>((ref) {
  final controller = AuthController(ref.watch(tokenStoreProvider));
  ref.onDispose(controller.dispose);
  return controller;
});

final apiClientProvider = Provider<ApiClient>(
  (ref) => ApiClient(
    tokenStore: ref.watch(tokenStoreProvider),
    auth: ref.watch(authControllerProvider),
  ),
);

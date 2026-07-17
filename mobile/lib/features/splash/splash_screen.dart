import 'package:flutter/material.dart';

/// Shown while [AuthController.bootstrap] reads the stored token. The router moves off
/// this as soon as `isBootstrapped` flips.
class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(body: Center(child: CircularProgressIndicator()));
  }
}

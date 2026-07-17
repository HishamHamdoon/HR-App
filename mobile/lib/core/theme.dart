import 'package:flutter/material.dart';

/// App theme. A single seed colour drives light and dark Material 3 schemes; screens
/// pull every colour from `Theme.of(context).colorScheme` so both modes come for free.
class AppTheme {
  const AppTheme._();

  static const _seed = Color(
    0xFF0D6EFD,
  ); // Bootstrap primary, to echo the web app.

  static ThemeData get light => _build(Brightness.light);
  static ThemeData get dark => _build(Brightness.dark);

  static ThemeData _build(Brightness brightness) {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: _seed,
        brightness: brightness,
      ),
      appBarTheme: const AppBarTheme(centerTitle: false),
    );
  }
}

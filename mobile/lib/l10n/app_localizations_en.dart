// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppL10nEn extends AppL10n {
  AppL10nEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'HR-App';

  @override
  String get loading => 'Loading…';

  @override
  String get loginTitle => 'Sign in';

  @override
  String get username => 'Username';

  @override
  String get password => 'Password';

  @override
  String get signIn => 'Sign in';

  @override
  String get changePasswordTitle => 'Change password';

  @override
  String get currentPassword => 'Current password';

  @override
  String get newPassword => 'New password';

  @override
  String get confirmPassword => 'Confirm new password';

  @override
  String get save => 'Save';

  @override
  String get cancel => 'Cancel';

  @override
  String get retry => 'Retry';

  @override
  String get signOut => 'Sign out';

  @override
  String get homeTitle => 'Home';

  @override
  String welcome(String name) {
    return 'Welcome, $name';
  }

  @override
  String get fieldRequired => 'This field is required.';

  @override
  String get passwordsDoNotMatch => 'The passwords do not match.';

  @override
  String get passwordChangedSignInAgain =>
      'Password changed. Please sign in with your new password.';

  @override
  String get mustChangePasswordNotice =>
      'You must set a new password before continuing.';

  @override
  String get profileTitle => 'My profile';

  @override
  String get editProfile => 'Edit profile';

  @override
  String get phone => 'Phone';

  @override
  String get address => 'Address';

  @override
  String get email => 'Email';

  @override
  String get department => 'Department';

  @override
  String get jobTitle => 'Job title';

  @override
  String get country => 'Country';

  @override
  String get manager => 'Manager';

  @override
  String get birthDate => 'Date of birth';

  @override
  String get hireDate => 'Hire date';

  @override
  String get profileUpdated => 'Profile updated.';

  @override
  String get notProvided => '—';
}

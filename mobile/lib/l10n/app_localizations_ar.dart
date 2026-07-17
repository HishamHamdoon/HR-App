// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Arabic (`ar`).
class AppL10nAr extends AppL10n {
  AppL10nAr([String locale = 'ar']) : super(locale);

  @override
  String get appTitle => 'تطبيق الموارد البشرية';

  @override
  String get loading => 'جارٍ التحميل…';

  @override
  String get loginTitle => 'تسجيل الدخول';

  @override
  String get username => 'اسم المستخدم';

  @override
  String get password => 'كلمة المرور';

  @override
  String get signIn => 'تسجيل الدخول';

  @override
  String get changePasswordTitle => 'تغيير كلمة المرور';

  @override
  String get currentPassword => 'كلمة المرور الحالية';

  @override
  String get newPassword => 'كلمة المرور الجديدة';

  @override
  String get save => 'حفظ';

  @override
  String get signOut => 'تسجيل الخروج';

  @override
  String get homeTitle => 'الرئيسية';

  @override
  String welcome(String name) {
    return 'مرحبًا، $name';
  }
}

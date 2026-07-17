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
  String get confirmPassword => 'تأكيد كلمة المرور الجديدة';

  @override
  String get save => 'حفظ';

  @override
  String get cancel => 'إلغاء';

  @override
  String get retry => 'إعادة المحاولة';

  @override
  String get signOut => 'تسجيل الخروج';

  @override
  String get homeTitle => 'الرئيسية';

  @override
  String welcome(String name) {
    return 'مرحبًا، $name';
  }

  @override
  String get fieldRequired => 'هذا الحقل مطلوب.';

  @override
  String get passwordsDoNotMatch => 'كلمتا المرور غير متطابقتين.';

  @override
  String get passwordChangedSignInAgain =>
      'تم تغيير كلمة المرور. الرجاء تسجيل الدخول بكلمة المرور الجديدة.';

  @override
  String get mustChangePasswordNotice =>
      'يجب تعيين كلمة مرور جديدة قبل المتابعة.';

  @override
  String get profileTitle => 'ملفي الشخصي';

  @override
  String get editProfile => 'تعديل الملف الشخصي';

  @override
  String get phone => 'الهاتف';

  @override
  String get address => 'العنوان';

  @override
  String get email => 'البريد الإلكتروني';

  @override
  String get department => 'القسم';

  @override
  String get jobTitle => 'المسمى الوظيفي';

  @override
  String get country => 'الدولة';

  @override
  String get manager => 'المدير';

  @override
  String get birthDate => 'تاريخ الميلاد';

  @override
  String get hireDate => 'تاريخ التعيين';

  @override
  String get profileUpdated => 'تم تحديث الملف الشخصي.';

  @override
  String get notProvided => '—';
}

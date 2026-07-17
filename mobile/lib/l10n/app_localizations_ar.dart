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

  @override
  String get leaveTitle => 'الإجازات';

  @override
  String get leaveBalanceTab => 'الرصيد';

  @override
  String get leaveHistoryTab => 'السجل';

  @override
  String get requestLeave => 'طلب إجازة';

  @override
  String get leaveType => 'نوع الإجازة';

  @override
  String get startDate => 'تاريخ البداية';

  @override
  String get endDate => 'تاريخ النهاية';

  @override
  String get halfDay => 'نصف يوم';

  @override
  String get note => 'ملاحظة';

  @override
  String get attachment => 'مرفق';

  @override
  String get addAttachment => 'إضافة مرفق';

  @override
  String get attachmentRequired => 'يتطلب هذا النوع من الإجازة إرفاق مستند.';

  @override
  String get submit => 'إرسال';

  @override
  String remainingDays(String days) {
    return 'متبقّي $days';
  }

  @override
  String entitlementTakenRemaining(
    String entitlement,
    String taken,
    String remaining,
  ) {
    return 'المستحق $entitlement · المستخدم $taken · المتبقّي $remaining';
  }

  @override
  String get noLeaves => 'لا توجد لديك طلبات إجازة بعد.';

  @override
  String get noBalances => 'لا توجد أنواع إجازات مُعدّة.';

  @override
  String get statusPending => 'قيد الانتظار';

  @override
  String get statusApproved => 'مقبول';

  @override
  String get statusRejected => 'مرفوض';

  @override
  String get endBeforeStart =>
      'لا يمكن أن يكون تاريخ النهاية قبل تاريخ البداية.';

  @override
  String get halfDaySameDay =>
      'يجب أن تبدأ إجازة نصف اليوم وتنتهي في نفس اليوم.';

  @override
  String minDaysRequired(int min, String requested) {
    return 'يتطلب هذا النوع $min يوم على الأقل؛ لقد طلبت $requested.';
  }

  @override
  String maxDaysAllowed(int max, String requested) {
    return 'يسمح هذا النوع بحد أقصى $max يوم؛ لقد طلبت $requested.';
  }

  @override
  String get leaveSubmitted => 'تم إرسال طلب الإجازة.';

  @override
  String daysCount(String days) {
    return '$days يوم';
  }
}

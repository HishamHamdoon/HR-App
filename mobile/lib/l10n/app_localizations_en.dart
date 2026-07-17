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

  @override
  String get leaveTitle => 'Leave';

  @override
  String get leaveBalanceTab => 'Balance';

  @override
  String get leaveHistoryTab => 'History';

  @override
  String get requestLeave => 'Request leave';

  @override
  String get leaveType => 'Leave type';

  @override
  String get startDate => 'Start date';

  @override
  String get endDate => 'End date';

  @override
  String get halfDay => 'Half day';

  @override
  String get note => 'Note';

  @override
  String get attachment => 'Attachment';

  @override
  String get addAttachment => 'Add attachment';

  @override
  String get attachmentRequired => 'This leave type requires an attachment.';

  @override
  String get submit => 'Submit';

  @override
  String remainingDays(String days) {
    return '$days left';
  }

  @override
  String entitlementTakenRemaining(
    String entitlement,
    String taken,
    String remaining,
  ) {
    return '$entitlement entitled · $taken taken · $remaining left';
  }

  @override
  String get noLeaves => 'You have no leave requests yet.';

  @override
  String get noBalances => 'No leave types are configured.';

  @override
  String get statusPending => 'Pending';

  @override
  String get statusApproved => 'Approved';

  @override
  String get statusRejected => 'Rejected';

  @override
  String get endBeforeStart => 'The end date cannot be before the start date.';

  @override
  String get halfDaySameDay =>
      'A half-day leave must start and end on the same day.';

  @override
  String minDaysRequired(int min, String requested) {
    return 'This type needs at least $min day(s); you requested $requested.';
  }

  @override
  String maxDaysAllowed(int max, String requested) {
    return 'This type allows at most $max day(s); you requested $requested.';
  }

  @override
  String get leaveSubmitted => 'Leave request submitted.';

  @override
  String daysCount(String days) {
    return '$days day(s)';
  }

  @override
  String get teamLeavesTitle => 'Team leaves';

  @override
  String get myTeamTitle => 'My team';

  @override
  String get pendingTab => 'Pending';

  @override
  String get allTab => 'All';

  @override
  String get approve => 'Approve';

  @override
  String get reject => 'Reject';

  @override
  String get rejectReason => 'Reason for rejection';

  @override
  String get reasonRequired => 'A reason is required to reject.';

  @override
  String get leaveApproved => 'Leave approved.';

  @override
  String get leaveRejected => 'Leave rejected.';

  @override
  String get noPendingLeaves => 'No leaves are waiting for your decision.';

  @override
  String get noTeamLeaves => 'Your team has no leave requests.';

  @override
  String get noTeamMembers => 'You have no team members.';

  @override
  String approveConfirm(String name, String type) {
    return 'Approve $name\'s $type leave?';
  }
}

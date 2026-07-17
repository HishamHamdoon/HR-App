import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'auth/auth_controller.dart';
import 'providers.dart';
import '../features/auth/change_password_screen.dart';
import '../features/auth/login_screen.dart';
import '../features/home/home_screen.dart';
import '../features/leave/leave_screen.dart';
import '../features/manager/my_team_screen.dart';
import '../features/manager/team_leaves_screen.dart';
import '../features/notifications/notifications_screen.dart';
import '../features/profile/profile_screen.dart';
import '../features/splash/splash_screen.dart';

/// Route table plus the redirect that enforces session state.
///
/// The redirect mirrors the server's own gating: an unauthenticated user goes to login,
/// and a user whose token carries `MustChangePassword=true` is pinned to the change-
/// password screen (the API's LicenseEnforcementMiddleware does the same on the web).
final routerProvider = Provider<GoRouter>((ref) {
  final auth = ref.watch(authControllerProvider);

  return GoRouter(
    initialLocation: Routes.splash,
    refreshListenable: auth,
    redirect: (context, state) => _redirect(auth, state),
    routes: [
      GoRoute(path: Routes.splash, builder: (_, _) => const SplashScreen()),
      GoRoute(path: Routes.login, builder: (_, _) => const LoginScreen()),
      GoRoute(
        path: Routes.changePassword,
        builder: (_, _) => const ChangePasswordScreen(),
      ),
      GoRoute(path: Routes.home, builder: (_, _) => const HomeScreen()),
      GoRoute(path: Routes.profile, builder: (_, _) => const ProfileScreen()),
      GoRoute(path: Routes.leave, builder: (_, _) => const LeaveScreen()),
      GoRoute(
        path: Routes.teamLeaves,
        builder: (_, _) => const TeamLeavesScreen(),
      ),
      GoRoute(path: Routes.myTeam, builder: (_, _) => const MyTeamScreen()),
      GoRoute(
        path: Routes.notifications,
        builder: (_, _) => const NotificationsScreen(),
      ),
    ],
  );
});

String? _redirect(AuthController auth, GoRouterState state) {
  final loc = state.matchedLocation;

  // Hold on the splash until storage has been read, so we don't flash login.
  if (!auth.isBootstrapped) {
    return loc == Routes.splash ? null : Routes.splash;
  }

  final loggedIn = auth.isLoggedIn;
  if (!loggedIn) {
    return loc == Routes.login ? null : Routes.login;
  }

  if (auth.mustChangePassword) {
    return loc == Routes.changePassword ? null : Routes.changePassword;
  }

  // Logged in and healthy: keep off the pre-auth screens. change-password is NOT bounced
  // here — a healthy user may open it voluntarily; the forced case is handled above.
  if (loc == Routes.login || loc == Routes.splash) {
    return Routes.home;
  }
  return null;
}

class Routes {
  const Routes._();
  static const splash = '/';
  static const login = '/login';
  static const changePassword = '/change-password';
  static const home = '/home';
  static const profile = '/profile';
  static const leave = '/leave';
  static const teamLeaves = '/team-leaves';
  static const myTeam = '/my-team';
  static const notifications = '/notifications';
}

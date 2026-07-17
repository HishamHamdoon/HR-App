import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/features/leave/leave_calc.dart';

/// These must match Emp.Api.Services.LeaveCalculations exactly, or client-side validation
/// disagrees with the server.
void main() {
  group('daysInclusive', () {
    test('single day is 1', () {
      expect(daysInclusive(DateTime(2026, 7, 1), DateTime(2026, 7, 1)), 1);
    });
    test('inclusive range', () {
      expect(daysInclusive(DateTime(2026, 7, 1), DateTime(2026, 7, 5)), 5);
    });
    test('reversed range clamps to 0', () {
      expect(daysInclusive(DateTime(2026, 7, 5), DateTime(2026, 7, 1)), 0);
    });
    test('ignores time of day', () {
      expect(
        daysInclusive(DateTime(2026, 7, 1, 23), DateTime(2026, 7, 2, 1)),
        2,
      );
    });
  });

  group('effectiveDays', () {
    test('half-day on a single day is 0.5', () {
      expect(
        effectiveDays(DateTime(2026, 7, 1), DateTime(2026, 7, 1), true),
        0.5,
      );
    });
    test('half-day flag across two days falls back to inclusive count', () {
      // The server only honours half-day when start and end are the same day.
      expect(
        effectiveDays(DateTime(2026, 7, 1), DateTime(2026, 7, 2), true),
        2,
      );
    });
    test('normal multi-day', () {
      expect(
        effectiveDays(DateTime(2026, 7, 1), DateTime(2026, 7, 3), false),
        3,
      );
    });
  });
}

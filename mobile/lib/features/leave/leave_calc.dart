// Client-side mirror of the server's leave math (Emp.Api.Services.LeaveCalculations),
// so the request form can validate before a round-trip. The server stays authoritative.

/// Inclusive day count; a single day is 1. Never negative.
int daysInclusive(DateTime start, DateTime end) {
  final s = _dateOnly(start);
  final e = _dateOnly(end);
  final days = e.difference(s).inDays + 1;
  return days < 0 ? 0 : days;
}

/// Days charged against the balance: a single-day request flagged half-day counts as 0.5,
/// otherwise the inclusive day count. Matches `LeaveCalculations.EffectiveDays`.
double effectiveDays(DateTime start, DateTime end, bool isHalfDay) {
  return (isHalfDay && _sameDay(start, end))
      ? 0.5
      : daysInclusive(start, end).toDouble();
}

bool _sameDay(DateTime a, DateTime b) =>
    a.year == b.year && a.month == b.month && a.day == b.day;

/// Local date-only (drops time) so day counts don't drift across a DST/hour boundary.
DateTime _dateOnly(DateTime d) => DateTime(d.year, d.month, d.day);

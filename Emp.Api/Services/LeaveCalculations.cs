namespace Emp.Api.Services
{
    /// <summary>Pure leave math, extracted so it can be unit-tested without a database.</summary>
    public static class LeaveCalculations
    {
        /// <summary>Inclusive day count between two dates (a single-day leave counts as 1).</summary>
        public static int DaysInclusive(DateTime start, DateTime end)
        {
            var days = (end.Date - start.Date).Days + 1;
            return days < 0 ? 0 : days;
        }

        /// <summary>Remaining entitlement, never negative.</summary>
        public static int Remaining(int entitlement, int taken)
            => Math.Max(entitlement - taken, 0);

        /// <summary>Remaining entitlement (decimal, for half-day support), never negative.</summary>
        public static decimal Remaining(decimal entitlement, decimal taken)
            => Math.Max(entitlement - taken, 0m);

        /// <summary>
        /// Days charged against the balance. A single-day request flagged half-day counts as 0.5;
        /// otherwise the inclusive day count.
        /// </summary>
        public static decimal EffectiveDays(DateTime start, DateTime end, bool isHalfDay)
            => (isHalfDay && start.Date == end.Date) ? 0.5m : DaysInclusive(start, end);
    }
}

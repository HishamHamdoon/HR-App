using Emp.Api.Services;
using Xunit;

namespace Emp.Tests
{
    public class LeaveCalculationsTests
    {
        [Theory]
        [InlineData("2026-01-01", "2026-01-01", 1)]   // single day is inclusive
        [InlineData("2026-01-01", "2026-01-05", 5)]
        [InlineData("2026-01-01", "2026-01-31", 31)]
        [InlineData("2026-01-05", "2026-01-01", 0)]   // end before start -> 0, never negative
        public void DaysInclusive_Counts_Both_Ends(string start, string end, int expected)
        {
            var s = DateTime.Parse(start);
            var e = DateTime.Parse(end);
            Assert.Equal(expected, LeaveCalculations.DaysInclusive(s, e));
        }

        [Theory]
        [InlineData(20, 5, 15)]
        [InlineData(20, 20, 0)]
        [InlineData(20, 25, 0)]   // over-taken clamps to 0
        [InlineData(0, 0, 0)]
        public void Remaining_Never_Goes_Negative(int entitlement, int taken, int expected)
        {
            Assert.Equal(expected, LeaveCalculations.Remaining(entitlement, taken));
        }

        [Fact]
        public void EffectiveDays_Single_Day_HalfDay_Is_Half()
        {
            var d = DateTime.Parse("2026-01-01");
            Assert.Equal(0.5m, LeaveCalculations.EffectiveDays(d, d, isHalfDay: true));
        }

        [Fact]
        public void EffectiveDays_Single_Day_FullDay_Is_One()
        {
            var d = DateTime.Parse("2026-01-01");
            Assert.Equal(1m, LeaveCalculations.EffectiveDays(d, d, isHalfDay: false));
        }

        [Fact]
        public void EffectiveDays_HalfDay_Ignored_For_Multi_Day_Range()
        {
            // Half-day only applies to a single day; a multi-day range counts in full.
            var s = DateTime.Parse("2026-01-01");
            var e = DateTime.Parse("2026-01-03");
            Assert.Equal(3m, LeaveCalculations.EffectiveDays(s, e, isHalfDay: true));
        }

        [Theory]
        [InlineData(20, 19.5, 0.5)]
        [InlineData(20, 20, 0)]
        [InlineData(20, 25, 0)]
        public void Remaining_Decimal_Never_Goes_Negative(decimal entitlement, decimal taken, decimal expected)
        {
            Assert.Equal(expected, LeaveCalculations.Remaining(entitlement, taken));
        }
    }
}

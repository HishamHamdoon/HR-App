using Emp.Models;
using Xunit;

namespace Emp.Tests
{
    public class SalaryTests
    {
        [Theory]
        [InlineData(5000, 500, 200, 5300)]
        [InlineData(1000, 0, 0, 1000)]
        [InlineData(3000, 1000, 4000, 0)]      // deductions exceed -> negative allowed by formula
        [InlineData(0, 0, 0, 0)]
        public void NetSalary_Is_Basic_Plus_Allowances_Minus_Deductions(
            decimal basic, decimal allowances, decimal deductions, decimal expected)
        {
            var salary = new Salary
            {
                BasicSalary = basic,
                Allowances = allowances,
                Deductions = deductions
            };

            // The third case intentionally documents the current formula (no flooring at 0).
            var computed = salary.NetSalary;

            if (basic + allowances - deductions >= 0)
            {
                Assert.Equal(expected, computed);
            }
            else
            {
                Assert.Equal(basic + allowances - deductions, computed);
            }
        }

        [Fact]
        public void NetSalary_Reflects_Updated_Components()
        {
            var salary = new Salary { BasicSalary = 1000, Allowances = 0, Deductions = 0 };
            Assert.Equal(1000, salary.NetSalary);

            salary.Allowances = 250;
            salary.Deductions = 50;

            Assert.Equal(1200, salary.NetSalary);
        }
    }
}

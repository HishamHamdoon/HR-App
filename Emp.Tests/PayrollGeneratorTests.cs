using Emp.Api.Models;
using Emp.Api.Services;
using Emp.Models;
using Xunit;

namespace Emp.Tests
{
    public class PayrollGeneratorTests
    {
        private static Employee EmployeeWithSalary(decimal basic, decimal allowances, decimal deductions) =>
            new Employee
            {
                Id = 1,
                Name = "Test",
                Salary = new Salary
                {
                    Id = 10,
                    BasicSalary = basic,
                    Allowances = allowances,
                    Deductions = deductions
                }
            };

        [Theory]
        [InlineData(5000, 500, 200)]
        [InlineData(1000, 0, 0)]
        [InlineData(8000, 1500, 1000)]
        public void Gross_Minus_Deductions_Equals_Net(decimal basic, decimal allowances, decimal deductions)
        {
            var emp = EmployeeWithSalary(basic, allowances, deductions);

            var payroll = PayrollGenerator.BuildPayroll(emp, new DateTime(2026, 5, 1));

            Assert.Equal(basic + allowances, payroll.GrossSalary);          // gross includes allowances
            Assert.Equal(deductions, payroll.Deductions);                   // real deductions, not 0
            Assert.Equal(payroll.GrossSalary - payroll.Deductions, payroll.NetSalary); // reconciles
            Assert.False(payroll.IsPaid);                                   // not paid at generation
            Assert.Equal(10, payroll.SalaryId);
            Assert.Equal(new DateTime(2026, 5, 1), payroll.SalaryMonth);    // normalized to first of month
        }

        [Fact]
        public void SalaryMonth_Is_Normalized_To_First_Of_Month()
        {
            var emp = EmployeeWithSalary(1000, 0, 0);
            var payroll = PayrollGenerator.BuildPayroll(emp, new DateTime(2026, 5, 23));
            Assert.Equal(new DateTime(2026, 5, 1), payroll.SalaryMonth);
        }
    }
}

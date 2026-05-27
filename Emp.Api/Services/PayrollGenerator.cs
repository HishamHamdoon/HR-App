using Emp.Api.Data;
using Emp.Api.Models;
using Emp.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Services
{
    /// <summary>
    /// Single source of truth for payroll generation. Both the manual endpoint
    /// (PayrollsController) and the scheduled job (PayrollJobService) use this so
    /// the math can never drift between them.
    /// </summary>
    public static class PayrollGenerator
    {
        /// <summary>Builds a payroll snapshot for an employee for the given month.</summary>
        public static Payroll BuildPayroll(Employee employee, DateTime month)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var salary = employee.Salary!;
            return new Payroll
            {
                EmployeeId = employee.Id,
                SalaryId = salary.Id,
                GrossSalary = salary.BasicSalary + salary.Allowances,
                Deductions = salary.Deductions,
                NetSalary = salary.NetSalary, // Basic + Allowances - Deductions; reconciles with Gross - Deductions
                SalaryMonth = firstDay,
                GeneratedAt = DateTime.UtcNow,
                IsPaid = false
            };
        }

        /// <summary>
        /// Generates payroll rows for all employees with a salary for the given month,
        /// skipping any that already exist. Returns the number of rows created.
        /// </summary>
        public static async Task<int> GenerateForMonthAsync(AppDbContext db, DateTime month, CancellationToken ct = default)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);

            var employees = await db.Employees
                .Include(e => e.Salary)
                .Where(e => e.Salary != null)
                .ToListAsync(ct);

            var created = 0;
            foreach (var emp in employees)
            {
                var exists = await db.Payrolls
                    .AnyAsync(p => p.EmployeeId == emp.Id && p.SalaryMonth == firstDay, ct);
                if (exists) continue;

                db.Payrolls.Add(BuildPayroll(emp, firstDay));
                created++;
            }

            await db.SaveChangesAsync(ct);
            return created;
        }
    }
}

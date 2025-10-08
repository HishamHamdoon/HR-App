
using Emp.Api.Data;
using Emp.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Services.JobServices
{
    public class PayrollJobService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PayrollJobService> _logger;

        public PayrollJobService(IServiceProvider serviceProvider, ILogger<PayrollJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var employees = await dbContext.Employees
                            .Include(e => e.Salary)
                            .ToListAsync(stoppingToken);

                        foreach (var emp in employees)
                        {
                            if (emp.Salary == null) continue;

                            bool exists = await dbContext.Payrolls.AnyAsync(
                                p => p.EmployeeId == emp.Id && p.SalaryMonth == firstDayOfMonth,
                                stoppingToken);

                            if (!exists)
                            {
                                var payroll = new Payroll
                                {
                                    EmployeeId = emp.Id,
                                    SalaryId = emp.Salary.Id,
                                    GrossSalary = emp.Salary.BasicSalary + emp.Salary.Allowances,
                                    Deductions = emp.Salary.Deductions,
                                    NetSalary = emp.Salary.NetSalary,
                                    SalaryMonth = firstDayOfMonth,
                                    GeneratedAt = DateTime.UtcNow,
                                    IsPaid = false
                                };

                                dbContext.Payrolls.Add(payroll);
                            }
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    _logger.LogInformation("✅ Payroll job executed at {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error while generating payroll");
                }

                // Sleep until tomorrow
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

}

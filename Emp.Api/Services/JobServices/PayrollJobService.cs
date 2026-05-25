
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
                        var created = await Services.PayrollGenerator.GenerateForMonthAsync(dbContext, firstDayOfMonth, stoppingToken);
                        _logger.LogInformation("Payroll job executed at {time}; {count} record(s) created.", DateTime.UtcNow, created);
                    }
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

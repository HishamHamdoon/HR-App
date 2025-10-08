using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos.Employee;
using Emp.Api.Dtos;
using Emp.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ResponseDto _response;
        private readonly IMapper _mapper;

        private readonly List<EmployeeDto> _employees;

        public PayrollsController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _employees = new List<EmployeeDto>();

        }

        [HttpPost("generate-monthly-payroll")]
        public async Task GenerateMonthlyPayroll(DateTime forMonth)
        {
            var response = new ResponseDto();
            var employees = await _dbContext.Employees
                .Include(e => e.Salary)
                .ToListAsync();

            foreach (var emp in employees)
            {
                if (emp.Salary == null) continue;

                var exists = await _dbContext.Payrolls
                    .AnyAsync(p => p.EmployeeId == emp.Id && p.SalaryMonth.Month == forMonth.Month && p.SalaryMonth.Year == forMonth.Year);

                if (!exists)
                {
                    var payroll = new Payroll
                    {
                        EmployeeId = emp.Id,
                        GrossSalary = emp.Salary.BasicSalary,
                        Deductions = 0, // you can add tax/leave deductions here
                        NetSalary = emp.Salary.NetSalary,
                        SalaryMonth = new DateTime(forMonth.Year, forMonth.Month, 1),
                        GeneratedAt = DateTime.Now,
                        IsPaid = true
                    };

                    _dbContext.Payrolls.Add(payroll);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
        [HttpPost]
        public async Task<bool> PaySalary(int payrollId)
        {
            var payroll = await _dbContext.Payrolls.FindAsync(payrollId);
            if (payroll == null) return false;

            payroll.IsPaid = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        [HttpGet("current-month-payrolls")]
        public async Task<ResponseDto> GetCurrentMonthPayrolls()
        {
            var response = new ResponseDto();
            try
            {
                var now = DateTime.UtcNow;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

                var payrolls = await _dbContext.Payrolls
                    .Include(p => p.Employee) // to get employee details
                    //.Where(p => p.SalaryMonth == firstDayOfMonth)
                    .Select(p => new PayrollDto
                    {
                        Id = p.Id,
                        EmployeeId = p.EmployeeId,
                        EmployeeName = p.Employee != null ? p.Employee.Name : string.Empty,
                        GrossSalary = p.GrossSalary,
                        Deductions = p.Deductions,
                        NetSalary = p.NetSalary,
                        SalaryMonth = p.SalaryMonth,
                        IsPaid = p.IsPaid,
                        GeneratedAt = p.GeneratedAt
                    })
                    .ToListAsync();

                response.Result = payrolls;
                response.IsSuccess = true;
                response.Message = "";
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

    }
}

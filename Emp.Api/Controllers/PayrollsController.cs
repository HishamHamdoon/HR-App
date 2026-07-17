using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos.Employee;
using Emp.Api.Dtos;
using Emp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    // Base policy is any authenticated user; every admin action re-asserts the Admin role,
    // and only the self-service "mine" action is open to any employee.
    [Authorize]
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

        [Authorize(Roles = "Admin")]
        [HttpPost("generate-monthly-payroll")]
        public async Task<ResponseDto> GenerateMonthlyPayroll(DateTime forMonth)
        {
            var response = new ResponseDto();
            try
            {
                var created = await Services.PayrollGenerator.GenerateForMonthAsync(_dbContext, forMonth);
                response.IsSuccess = true;
                response.Result = new { Created = created, Month = new DateTime(forMonth.Year, forMonth.Month, 1) };
                response.Message = created == 0
                    ? "Payroll already generated for this month."
                    : $"Generated {created} payroll record(s).";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
            }
            return response;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<bool> PaySalary(int payrollId)
        {
            var payroll = await _dbContext.Payrolls.FindAsync(payrollId);
            if (payroll == null) return false;

            payroll.IsPaid = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// The signed-in employee's own payroll history (self-service), newest first.
        /// Returns an empty list when none have been generated.
        /// </summary>
        [Authorize]
        [HttpGet("mine")]
        public async Task<ResponseDto> GetMyPayrolls()
        {
            var response = new ResponseDto();
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId);
            if (employeeId == 0)
            {
                response.IsSuccess = false;
                response.Message = "No employee is associated with this account.";
                return response;
            }

            var payrolls = await _dbContext.Payrolls
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.SalaryMonth)
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
            return response;
        }

    }
}

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
    [Authorize(Roles = "Admin")]
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

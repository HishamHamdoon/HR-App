using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Salary;
using Emp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    // Base policy is any authenticated user; every admin action re-asserts the Admin role
    // explicitly, and only the self-service "mine" action is left open to any employee.
    // (A class-level Admin requirement cannot be relaxed by an action-level [Authorize].)
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalaryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public SalaryController(AppDbContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;

        }
        /// <summary>
        /// List of Salary
        /// </summary>
        /// <returns>ResponseDto object whcih containse List of Salary in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ResponseDto> GetSalaries()
        {
            ResponseDto response = new();
            try
            {
                var salariesList = _context.Salaries
                .Include(s => s.Employee)
                    .Select(s => new SalaryDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        EmployeeName = s.Employee != null ? s.Employee.Name : string.Empty,
                        BasicSalary = s.BasicSalary,
                        Allowances = s.Allowances,
                        Deductions = s.Deductions,
                        NetSalary = s.NetSalary,
                        EffectiveDate = s.EffectiveDate
                    }).ToList();

                response.Result = _mapper.Map<List<SalaryDto>>(salariesList);
                response.IsSuccess = true;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }

        }

        /// <summary>
        /// Get single salary based on Id
        /// </summary>
        /// <param name="id">salary Id</param>
        /// <returns>ResponseDto which contains Single Salary in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ResponseDto GetSalary(int id)
        {
            ResponseDto response = new ResponseDto();
            try
            {
                //var salary = _context.Salaries
                //    .Include(s => s.Employee) // eager load Employee
                //    .FirstOrDefault(s => s.Id == id);
                var salary = _context.Salaries.Where(a=>a.Id==id)
                .Include(s => s.Employee)
                    .Select(s => new SalaryDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        EmployeeName = s.Employee != null ? s.Employee.Name : string.Empty,
                        BasicSalary = s.BasicSalary,
                        Allowances = s.Allowances,
                        Deductions = s.Deductions,
                        NetSalary = s.NetSalary,
                        EffectiveDate = s.EffectiveDate
                    }).FirstOrDefault();
                if (salary == null)
                {
                    response.Result = null;
                    response.IsSuccess = false;
                    response.Message = "No Data";
                    return response;
                }

                // Use AutoMapper with null-check for EmployeeName
                var salaryDto = _mapper.Map<SalaryDto>(salary);
                //salaryDto.EmployeeName = salary.Employee?.Name ?? string.Empty;

                response.Result = salaryDto;
                response.IsSuccess = true;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }
        }


        /// <summary>
        /// Create new Salary based on supplied CreateSalaryDto
        /// </summary>
        /// <param name="dto">CreateSalaryDto to create</param>
        /// <returns>ResponseDto which contains created Salary in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateSalary")]

        public async Task<ResponseDto> CreateSalary(CreateSalaryDto dto)
        {
            ResponseDto response = new();
            try
            {
                // ✅ Validation: Employee exists
                var employee = await _context.Employees.FindAsync(dto.EmployeeId);
                if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee not found";
                    return response;
                }
                // ✅ Validation: Only one active salary per employee
                var existingSalary = _context.Salaries
                    .Any(s => s.EmployeeId == dto.EmployeeId);
                if (existingSalary)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee already has a salary assigned";
                    return response;
                }
                // ✅ Auto calculate NetSalary
                var salary = new Salary
                {
                    EmployeeId = dto.EmployeeId,
                    BasicSalary = dto.BasicSalary,
                    Allowances = dto.Allowances,
                    Deductions = dto.Deductions,
                    EffectiveDate = dto.EffectiveDate
                };

                _context.Salaries.Add(salary);
                await _context.SaveChangesAsync();

                var createdSalary = new SalaryDto
                {
                    Id = salary.Id,
                    EmployeeId = salary.EmployeeId,
                    BasicSalary = salary.BasicSalary,
                    Allowances = salary.Allowances,
                    Deductions = salary.Deductions,
                    NetSalary = salary.NetSalary,
                    EffectiveDate = salary.EffectiveDate
                };
                response.Message = "Salary created Successfully";
                response.Result = createdSalary;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }

        }

        /// <summary>
        /// Update Salary based on supplied Salary Id and new UpdateSalaryDto obj
        /// </summary>
        /// <param name="id">salary Id</param>
        /// <param name="dto">UpdateSalaryDto obj</param>
        /// <returns>ResponseDto which contains updated Salary in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ResponseDto> UpdateSalary(UpdateSalaryDto dto)
        {
            ResponseDto response = new();
            try
            {
                var salary = await _context.Salaries.FindAsync(dto.Id);
                if (salary == null)
                {
                    response.Result = null;
                    response.IsSuccess = false;
                    response.Message = "Error";
                    return response;
                }
                // ✅ Validation: Employee exists
                var employee = await _context.Employees.FindAsync(dto.EmployeeId);
                if (employee == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee not found";
                    response.Result = false;
                    return response;
                }
                else
                {
                    salary.BasicSalary = dto.BasicSalary;
                    salary.Allowances = dto.Allowances;
                    salary.Deductions = dto.Deductions;
                    salary.EffectiveDate = dto.EffectiveDate;
                    await _context.SaveChangesAsync();
                    response.Result = dto;
                    response.IsSuccess = true;
                    response.Message = "Salary updated successfully";
                    return response;
                }


            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }

        }

        /// <summary>
        /// Dalete single Salary based on supplied Salary Id
        /// </summary>
        /// <param name="id">Salary Id</param>
        /// <returns>ResponseDto which contains deleted Salary in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> DeleteSalary(int id)
        {
            ResponseDto response = new();
            try
            {
                var salary = await _context.Salaries.FindAsync(id);
                if (salary == null)
                {
                    response.Result = null;
                    response.IsSuccess = false;
                    response.Message = "No data";
                    return response;
                }
                else
                {
                    _context.Salaries.Remove(salary);
                    await _context.SaveChangesAsync();
                    response.Result = new { salary.Id };
                    response.Message = "Salary deleted successfully";
                    response.IsSuccess = true;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }

        }

        /// <summary>
        /// The signed-in employee's own salary (self-service). Returns null in Result when
        /// no salary has been configured, so the client can show an empty state.
        /// </summary>
        [Authorize]
        [HttpGet("mine")]
        public async Task<ResponseDto> GetMySalary()
        {
            var response = new ResponseDto();
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId);
            if (employeeId == 0)
            {
                response.IsSuccess = false;
                response.Message = "No employee is associated with this account.";
                return response;
            }

            // Load the entity then map in memory: NetSalary is a [NotMapped] computed
            // property and cannot be evaluated inside an EF-translated projection.
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

            response.IsSuccess = true;
            response.Result = salary == null
                ? null
                : new SalaryDto
                {
                    Id = salary.Id,
                    EmployeeId = salary.EmployeeId,
                    EmployeeName = salary.Employee?.Name ?? string.Empty,
                    BasicSalary = salary.BasicSalary,
                    Allowances = salary.Allowances,
                    Deductions = salary.Deductions,
                    NetSalary = salary.NetSalary,
                    EffectiveDate = salary.EffectiveDate
                };
            response.Message = salary == null ? "No salary is configured for your account." : "";
            return response;
        }
    }
}
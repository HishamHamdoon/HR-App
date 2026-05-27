using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.ExtendedProperties;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Employee;
using Emp.Api.Models;
using Emp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private ResponseDto _response;


        public EmployeeController(AppDbContext dbContext, IMapper mapper, ILogger<EmployeeController> logger)
        {
            _dbContext = dbContext;
            _response = new ResponseDto();
            _mapper = mapper;
            _logger = logger;
        }
        // GET: api/<EmployeeController>
        //[HttpGet]
        //public async Task<ResponseDto> GetAll()

        //{
        //    //should retreive employees based on permission
        //    //show all for admin - show isActive only for others
        //    try
        //    {
        //        var EmployeeModeldel = await _dbContext.Employees
        //        .Include("Department")
        //        .Include("JobTitle").Include("Country").ToListAsync();
        //        if (EmployeeModeldel is null)
        //        {
        //            _logger.LogError("There is no data to show");
        //        }
        //        var response = _mapper.Map<List<EmployeeViewDto>>(EmployeeModeldel);
        //        _response.Result = response;
        //        _response.IsSuccess = true;
        //        _response.Message = "";
        //        return _response;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.Result = null;
        //        _response.Message = ex.Message.ToString();
        //        _response.IsSuccess = false;
        //    }
        //    return _response;
        //}
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ResponseDto> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                // Start with base query
                IQueryable<Employee> query = _dbContext.Employees
                    .Include(e => e.Department)
                    .Include(e => e.JobTitle)
                    .Include(e => e.Manager)
                    .Include(e => e.Country);

                // 🔹 Apply permission filter
                //var isAdmin = User.IsInRole("Admin"); // Or however you check role
                //if (!isAdmin)
                //{
                //    query = query.Where(e => e.IsActive);
                //}

                // 🔹 Count total for pagination
                var totalCount = await query.CountAsync();

                // 🔹 Fetch paged data
                var employees = await query
                    .OrderBy(e => e.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 🔹 Map to DTO
                var response = _mapper.Map<List<EmployeeViewDto>>(employees);

                // 🔹 Wrap with pagination metadata
                _response.Result = new
                {
                    Data = response,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
                _response.IsSuccess = true;
                _response.Message = "";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees");
                _response.Result = null;
                _response.Message = ex.Message;
                _response.IsSuccess = false;
                return _response;
            }
        }

        /// <summary>
        /// Employees that belong to the departments (and sub-departments) managed by the
        /// given manager. Accessible by an Admin or the manager themselves.
        /// </summary>
        [HttpGet("by-manager/{managerId}")]
        public async Task<ResponseDto> GetByManager(int managerId)
        {
            // A manager may only list their own team; admins may list any manager's.
            if (!User.IsInRole("Admin"))
            {
                int.TryParse(User.FindFirst("EmployeeId")?.Value, out var caller);
                if (caller != managerId)
                {
                    _response.IsSuccess = false;
                    _response.Message = "You are not authorized to view this team.";
                    return _response;
                }
            }

            try
            {
                // Walk the department tree owned by this manager (department + all sub-departments).
                var rootDeptIds = await _dbContext.Departments
                    .Where(d => d.ManagerId == managerId)
                    .Select(d => d.Id)
                    .ToListAsync();

                var deptIds = new HashSet<int>(rootDeptIds);
                var frontier = new Queue<int>(rootDeptIds);
                while (frontier.Count > 0)
                {
                    var current = frontier.Dequeue();
                    var children = await _dbContext.Departments
                        .Where(d => d.ParentDepartmentId == current)
                        .Select(d => d.Id)
                        .ToListAsync();
                    foreach (var childId in children)
                    {
                        if (deptIds.Add(childId))
                            frontier.Enqueue(childId);
                    }
                }

                var employees = await _dbContext.Employees
                    .Include(e => e.Department)
                    .Include(e => e.JobTitle)
                    .Include(e => e.Country)
                    .Where(e => deptIds.Contains(e.DepartmentId) && e.Id != managerId)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                _response.Result = _mapper.Map<List<EmployeeViewDto>>(employees);
                _response.IsSuccess = true;
                _response.Message = "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team for manager {ManagerId}", managerId);
                _response.Result = null;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        /// <summary>Self-service update of the caller's own contact details (phone/address).</summary>
        [HttpPut("my-profile")]
        public async Task<ResponseDto> UpdateMyProfile([FromBody] Dtos.Employee.UpdateProfileDto dto)
        {
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId);
            var employee = await _dbContext.Employees.FindAsync(employeeId);
            if (employee is null)
            {
                _response.IsSuccess = false;
                _response.Message = "Employee not found.";
                return _response;
            }

            employee.Phone = dto.Phone;
            employee.Address = dto.Address;
            await _dbContext.SaveChangesAsync();

            _response.IsSuccess = true;
            _response.Message = "Profile updated successfully.";
            _response.Result = new { employee.Id, employee.Phone, employee.Address };
            return _response;
        }

        // GET api/<EmployeeController>/5
        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(int id)
        {
            try
            {
                var EmployeeModel = await _dbContext.Employees
                    .Include(e => e.Department)
                    .Include(e => e.JobTitle)
                    .Include(e => e.Country).
                    FirstOrDefaultAsync(x => x.Id == id);
                if (EmployeeModel != null)
                {
                    var response = _mapper.Map<EmployeeViewDto>(EmployeeModel);
                    _response.Result = response;
                    _response.IsSuccess = true;
                    _response.Message = "";
                }
                else
                {
                    _response.Result = null;
                    _response.IsSuccess = false;
                    _response.Message = "Employee not found.";
                }
            }
            catch (Exception ex)
            {
                _response.Result = null;
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] EmployeeCreateDto createDto)
        {

            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.Result = null;
                _response.Message = "Invalid input data.";
                return _response;
            }

            try
            {
                var employee = _mapper.Map<Employee>(createDto);

                if (employee == null)
                {
                    _logger.LogError("Mapping from EmployeeCreateDto to Employee failed.");
                    _response.IsSuccess = false;
                    _response.Message = "An error occurred while creating the employee.";
                    return _response;
                }

                await _dbContext.Employees.AddAsync(employee);
                await _dbContext.SaveChangesAsync();

                var createdEmployee = _mapper.Map<EmployeeViewDto>(employee);

                _response.IsSuccess = true;
                _response.Result = createdEmployee;
                _response.Message = "Employee created Successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new employee.");
                _response.IsSuccess = false;
                _response.Message = "An unexpected error occurred.";
            }

            return _response;
        }


        //// PUT api/<EmployeeController>/5
        //[HttpPut("{id}")]
        //public async Task<ResponseDto> Put(int id, [FromBody] EmployeeUpdateDto employeeUpdateDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        _response.IsSuccess = false;
        //        _response.Result = null;
        //        _response.Message = "Invalid input data.";
        //        return _response;
        //    }
        //    var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        //    if (employee != null)
        //    {
        //        var mappedmodel = _mapper.Map<Employee>(employeeUpdateDto);
        //        mappedmodel.DepartmentId = employee.DepartmentId;
        //        mappedmodel.JobTitleId = employee.JobTitleId;
        //        _dbContext.Update(mappedmodel);
        //        await _dbContext.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        return BadRequest("employee notfound");
        //    }

        //    return NoContent();
        //}
        [Authorize(Roles = "Admin")]
        [HttpPut("update-employee/{id}")]
        public async Task<ResponseDto> Put(int id, [FromBody] EmployeeUpdateDto employeeUpdateDto)
        {

            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.Result = null;
                _response.Message = "Invalid input data.";
                return _response;
            }

            try
            {
                var existingEmployee = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);

                if (existingEmployee == null)
                {
                    _response.Message = "Employee not found.";
                    _response.IsSuccess = false;
                    return _response;
                }

                // Update the existingEmployee with new values
                _mapper.Map(employeeUpdateDto, existingEmployee);

                // If you want to ensure DepartmentId and JobTitleId remain unchanged
                // (though they should ideally be part of the update DTO)
                // existingEmployee.DepartmentId = existingEmployee.DepartmentId;
                // existingEmployee.JobTitleId = existingEmployee.JobTitleId;

                _dbContext.Employees.Update(existingEmployee);
                await _dbContext.SaveChangesAsync();

                _response.IsSuccess = true;
                _response.Message = "Employee updated successfully.";
                _response.Result = _mapper.Map<EmployeeViewDto>(existingEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the employee.");
                _response.IsSuccess = false;
                _response.Message = "An unexpected error occurred.";
            }

            return _response;
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(int id)
        {
            var response = new ResponseDto();

            try
            {
                var employeeModel = await _dbContext.Employees.FindAsync(id);

                if (employeeModel == null)
                {
                    _logger.LogWarning($"Employee with Id: {id} not found.");
                    response.IsSuccess = false;
                    response.Message = $"Employee with Id {id} not found.";
                    return response;
                }

                // The seeded super-admin must never be deleted (it's the system's break-glass account).
                if (string.Equals(employeeModel.Email, Data.DbInitializer.AdminEmail, StringComparison.OrdinalIgnoreCase))
                {
                    response.IsSuccess = false;
                    response.Result = null;
                    response.Message = "The system administrator account cannot be deleted.";
                    return response;
                }
                // nullify subordinates' manager
                var subordinates = _dbContext.Employees.Where(e => e.ManagerId == id);
                if (subordinates.Any())
                {
                    foreach (var s in subordinates)
                    {
                        s.ManagerId = null;
                    }
                }
                // Check if employee is manager of any department
                var isManager = await _dbContext.Departments.AnyAsync(d => d.ManagerId == id);
                if (isManager)
                {
                    response.IsSuccess = false;
                    response.Result = null;
                    response.Message = "Cannot delete this employee because they are assigned as a manager to a department.";
                    return response;
                }
                _dbContext.Employees.Remove(employeeModel);
                await _dbContext.SaveChangesAsync();

                response.IsSuccess = true;
                response.Result = true;
                response.Message = "Employee deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting employee with Id: {id}");
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred while deleting the employee.";
                response.Result = false;
            }

            return response;
        }
        //edit employee's department only
        //[HttpPatch("UpdateDepartment/{id}")]
        //public async Task<IActionResult> UpdateDepartment(int id, int depId)
        //{
        //    var employeeModel = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);
        //    var department = await _dbContext.Departments.FirstOrDefaultAsync(y => y.Id == depId);
        //    if (employeeModel is null)
        //        return NotFound("Employee is not exist!");
        //    if (department is null)
        //        return BadRequest("Department is not exist!");
        //    employeeModel.DepartmentId = depId;
        //    await _dbContext.SaveChangesAsync();
        //    return NoContent();

        //}
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateDepartment/{id}")]
        public async Task<ResponseDto> UpdateDepartment(int id, int depId)
        {
            var response = new ResponseDto();

            try
            {
                var employeeModel = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);
                if (employeeModel == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee does not exist.";
                    return response;
                }

                var department = await _dbContext.Departments.FirstOrDefaultAsync(y => y.Id == depId);
                if (department == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Department does not exist.";
                    return response;
                }

                employeeModel.DepartmentId = depId;
                await _dbContext.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Department updated successfully.";
                response.Result = true; // or return updated employee data if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while updating department for employee Id: {id}");
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred.";
                response.Result = false;
            }

            return response;
        }

        ////terminate employee
        //[HttpPatch("terminate/{id}")]
        //public async Task<ResponseDto> EmployeeTermination(int id)
        //{
        //    var response = new ResponseDto();
        //    var employeeModel = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);
        //    if (employeeModel is { } && !employeeModel.IsActive)
        //    {
        //        response.Message = "Employee is already terminated!";
        //        response.IsSuccess = false;
        //    }
        //    else
        //    {
        //        employeeModel.IsActive = false;
        //        employeeModel.LeavingDate = DateOnly.FromDateTime(DateTime.Now);
        //        //add - termination type 
        //        //add termination reason >table
        //        await _dbContext.SaveChangesAsync();
        //        response.Result= true;
        //        _response.IsSuccess=true;
        //        _response.Message = "Employee terminated successfully";
        //    }
        //    return response;

        //}
        [Authorize(Roles = "Admin")]
        [HttpPatch("terminate/{id}")]
        public async Task<ResponseDto> EmployeeTermination(int id, [FromBody] TerminationDto terminationRequest)
        {
            var response = new ResponseDto();

            // Fetch the employee model from the database
            var employeeModel = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);

            // Check if the employee exists
            if (employeeModel == null)
            {
                response.IsSuccess = false;
                response.Message = "Employee not found.";
                response.Result = null;
                return response;
            }

            // Check if the employee is already terminated
            if (!employeeModel.IsActive)
            {
                response.IsSuccess = false;
                response.Message = "Employee is already terminated!";
                response.Result = null;
                return response;
            }

            // The seeded super-admin must never be terminated.
            if (string.Equals(employeeModel.Email, Data.DbInitializer.AdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                response.IsSuccess = false;
                response.Message = "The system administrator account cannot be terminated.";
                response.Result = null;
                return response;
            }

            // Set the employee status to terminated
            employeeModel.IsActive = false;
            employeeModel.LeavingDate = DateOnly.FromDateTime(DateTime.Now);

            // Record the termination.
            var termination = new Termination
            {
                EmployeeId = id,
                TerminationType = terminationRequest.TerminationType,
                TerminationReason = terminationRequest.TerminationReason,
                DateTerminated = DateOnly.FromDateTime(DateTime.Now)
            };
            _dbContext.Terminations.Add(termination);

            // Clear management links so departments, teams and approvals don't dangle on a
            // terminated person.
            var managedDepartments = await _dbContext.Departments.Where(d => d.ManagerId == id).ToListAsync();
            foreach (var d in managedDepartments) d.ManagerId = null;

            var subordinates = await _dbContext.Employees.Where(e => e.ManagerId == id).ToListAsync();
            foreach (var s in subordinates) s.ManagerId = null;

            // Pending requests routed to this person fall back to Admin approval.
            var awaitingApproval = await _dbContext.Leaves
                .Where(l => l.ManagerId == id && l.Status.ToUpper() == "PENDING").ToListAsync();
            foreach (var l in awaitingApproval) l.ManagerId = null;

            // Auto-reject the terminated employee's own outstanding (non-decided) requests.
            var ownOutstanding = await _dbContext.Leaves
                .Where(l => l.EmployeeId == id && l.Status.ToUpper() != "APPROVED" && l.Status.ToUpper() != "REJECTED")
                .ToListAsync();
            foreach (var l in ownOutstanding)
            {
                l.Status = "REJECTED";
                l.DecisionNote = "Auto-rejected: employee terminated.";
                l.DecidedAt = DateTime.Now;
                l.UpdatedAt = DateTime.Now;
                l.IsModified = true;
            }

            // Disable the login so the terminated employee can no longer sign in.
            var appUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (appUser != null)
            {
                appUser.LockoutEnabled = true;
                appUser.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _dbContext.SaveChangesAsync();

            // Set success response
            response.IsSuccess = true;
            response.Message = "Employee terminated successfully.";
            response.Result = true;  // You can return more data if needed (e.g., updated employee)

            return response;
        }

        /// <summary>
        /// Reverses a termination: re-activates the employee, clears the leaving date and
        /// unlocks their login. Manager/team links are NOT restored (re-assign as needed).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("reactivate/{id}")]
        public async Task<ResponseDto> Reactivate(int id)
        {
            var response = new ResponseDto();

            var employee = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);
            if (employee == null)
            {
                response.IsSuccess = false;
                response.Message = "Employee not found.";
                return response;
            }
            if (employee.IsActive)
            {
                response.IsSuccess = false;
                response.Message = "Employee is already active.";
                return response;
            }

            employee.IsActive = true;
            employee.LeavingDate = null;

            // Unlock the login.
            var appUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (appUser != null)
            {
                appUser.LockoutEnd = null;
                appUser.AccessFailedCount = 0;
            }

            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Employee reactivated successfully.";
            response.Result = true;
            return response;
        }

        /// <summary>Terminated employees with their most recent termination details (for reporting).</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("terminations")]
        public async Task<ResponseDto> Terminations()
        {
            var response = new ResponseDto();
            try
            {
                var terminated = await _dbContext.Employees
                    .Where(e => !e.IsActive)
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        Department = e.Department != null ? e.Department.Name : "",
                        e.LeavingDate
                    })
                    .ToListAsync();

                var ids = terminated.Select(t => t.Id).ToList();
                var terminations = await _dbContext.Terminations
                    .Where(t => ids.Contains(t.EmployeeId))
                    .ToListAsync();

                var rows = terminated.Select(e =>
                {
                    var latest = terminations
                        .Where(t => t.EmployeeId == e.Id)
                        .OrderByDescending(t => t.Id)
                        .FirstOrDefault();
                    return new
                    {
                        e.Name,
                        e.Department,
                        TerminationType = latest?.TerminationType ?? "",
                        TerminationReason = latest?.TerminationReason ?? "",
                        DateTerminated = (latest != null ? latest.DateTerminated : e.LeavingDate)?.ToString("yyyy-MM-dd") ?? ""
                    };
                })
                .OrderByDescending(r => r.DateTerminated)
                .ToList();

                response.IsSuccess = true;
                response.Result = rows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching terminations");
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        /*
        [HttpPatch("terminate/{id}")]
        public async Task<ResponseDto> EmployeeTermination(int id, [FromBody] TerminationRequestDto terminationRequest)
        {
            var response = new ResponseDto();

            try
            {
                var employeeModel = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id);
                if (employeeModel == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee not found.";
                    return response;
                }

                if (!employeeModel.IsActive)
                {
                    response.IsSuccess = false;
                    response.Message = "Employee is already terminated.";
                    return response;
                }

                // Optionally, you might want to create a Termination record if it's in a separate table
                if (terminationRequest != null)
                {
                    // Add termination reason and type to the employee record or a separate termination table
                    // Assuming you have a TerminationReason entity in your DB
                    var termination = new Termination
                    {
                        EmployeeId = id,
                        TerminationType = terminationRequest.TerminationType,
                        TerminationReason = terminationRequest.TerminationReason,
                        DateTerminated = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _dbContext.Terminations.Add(termination); // If you have a Terminations table
                }

                employeeModel.IsActive = false;
                employeeModel.LeavingDate = DateOnly.FromDateTime(DateTime.Now);

                await _dbContext.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Employee terminated successfully.";
                response.Result = true; // You can return any other result, like the updated employee model or termination info

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while terminating employee Id: {id}");
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred while terminating the employee.";
                response.Result = false;
            }

            return response;
        }
        */
        //set Manager
        [Authorize(Roles = "Admin")]
        [HttpPatch("SetManager")]
        public async Task<IActionResult> SetManager(int employeeId, int managerId)
        {
            try
            {
                if (employeeId <= 0 || managerId <= 0)
                {
                    return BadRequest(
                        new ResponseDto
                        {
                            IsSuccess = false,
                            Message = "Employee data is required.",
                        });
                }

                var emp = await _dbContext.Employees.AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.Id == employeeId);
                if (emp == null)
                {
                    return NotFound(new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "Employee not found.",
                    });
                }

                var manager = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == managerId);
                if (manager == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Manager not found." });
                }
                if (emp.ManagerId != null)
                {
                    emp.ManagerId = managerId;
                    await _dbContext.SaveChangesAsync();
                }
                emp.ManagerId = managerId;

                // Reattach the entity since AsNoTracking was used
                _dbContext.Employees.Update(emp);
                await _dbContext.SaveChangesAsync();

                return Ok(new ResponseDto { IsSuccess = true, Result = true, Message = "Manager set successfully." });
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, "An error occurred while setting the manager.");
            }
        }
        //set department's manager
        [Authorize(Roles = "Admin")]
        [HttpPatch("SetDepartmentManager")]
        public async Task<IActionResult> SetDepartmentManager(int departmentId, int managerId)
        {
            try
            {
                if (departmentId <= 0 || managerId <= 0)
                {
                    return BadRequest("Department and Manager data are required.");
                }
                var department = await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId);
                if (department == null)
                {
                    return NotFound("Department not found.");
                }
                var manager = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == managerId);
                if (manager == null)
                {
                    return NotFound("Manager not found.");
                }
                department.ManagerId = managerId;
                // Reattach the entity since AsNoTracking was used
                _dbContext.Departments.Update(department);
                await _dbContext.SaveChangesAsync();
                return Ok("Department manager set successfully.");
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, "An error occurred while setting the department manager.");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("active-deactive-employee/{employeeId}")]
        public async Task<ResponseDto> ActiveDeActiveEmployee(int employeeId)
        {
            var response = new ResponseDto();
            try
            {
                var employee = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == employeeId);
                if (employee != null)
                {
                    if (employee.IsActive)
                    {
                        employee.IsActive = false;
                        await _dbContext.SaveChangesAsync();
                        response.Result = true;
                        response.IsSuccess = true;
                        response.Message = "Employee is dactivated successfully";
                    }
                    else
                    {
                        employee.IsActive = true;
                        await _dbContext.SaveChangesAsync();
                        response.Result = true;
                        response.IsSuccess = true;
                        response.Message = "Employee is activated successfully";
                    }
                    return response;
                }
                else
                {
                    response.Result = false;
                    response.IsSuccess = false;
                    response.Message = "Employee is not found";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = false;
                return response;
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("/dashboard-counts")]
        public async Task<ResponseDto> GetDashboardCounts()
        {
            var response = new ResponseDto();
            try
            {
                int totalEmployees = await _dbContext.Employees.CountAsync();
                int activeLeaves = await _dbContext.Leaves.CountAsync(l => l.Status == "Approved"); // example
                int pendeingApprovals = await _dbContext.Leaves.CountAsync(l => l.Status == "PENDING"); // example
                int departments = await _dbContext.Departments.CountAsync();

                var result = new
                {
                    TotalEmployees = totalEmployees,
                    ActiveLeaves = activeLeaves,
                    Departments = departments,
                    PendingApprovals = pendeingApprovals
                };
                response.Result = result;
                response.IsSuccess = true;
                response.Message = string.Empty;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = null;
                return response;
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("/dashboard-charts")]
        public async Task<ResponseDto> GetDashboardCharts()
        {
            var response = new ResponseDto();
            try
            {
                // Leaves per month for the last 6 months (by StartDate).
                var firstMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
                var rawLeaves = await _dbContext.Leaves
                    .Where(l => l.StartDate >= firstMonth)
                    .Select(l => new { l.StartDate.Year, l.StartDate.Month })
                    .ToListAsync();

                var leavesPerMonth = new List<object>();
                for (int i = 0; i < 6; i++)
                {
                    var m = firstMonth.AddMonths(i);
                    var count = rawLeaves.Count(x => x.Year == m.Year && x.Month == m.Month);
                    leavesPerMonth.Add(new { Label = m.ToString("MMM"), Count = count });
                }

                // Employees per department.
                var employeesPerDepartment = await _dbContext.Employees
                    .Where(e => e.Department != null)
                    .GroupBy(e => e.Department.Name)
                    .Select(g => new { Label = g.Key, Count = g.Count() })
                    .ToListAsync();

                response.Result = new
                {
                    LeavesPerMonth = leavesPerMonth,
                    EmployeesPerDepartment = employeesPerDepartment
                };
                response.IsSuccess = true;
                response.Message = string.Empty;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        [HttpGet("GetManagerName/{employeeId}")]
        public async Task<ResponseDto?> GetManagerName(int employeeId)
        {
            var response = new ResponseDto();
            var managerName = await _dbContext.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => e.Manager != null
                    ? e. Manager.Id                   // direct manager
                    : e.Department.Manager.Id)       // department manager
                .FirstOrDefaultAsync();
            if (managerName != null)
            {
                response.Result = managerName;
                response.Message = "";
                response.IsSuccess=false;
            }
            else
            {
                response.Result = null;
                response.Message = "There is no manager";
                response.IsSuccess = false;
            }
            return response;
        }
    }
}
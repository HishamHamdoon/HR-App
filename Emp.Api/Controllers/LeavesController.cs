using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Leave;
using Emp.Api.Models;
using Emp.Api.Services.IServices;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeavesController : ControllerBase
    {

        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private string leave_not_found = "Leave not found";
        private readonly IWebHostEnvironment _hostEnvironment;
        IFileService _fileService;

        public LeavesController(AppDbContext dbContext,
            IMapper mapper, 
            IWebHostEnvironment hostEnvironment, 
            IFileService fileService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
            _fileService = fileService;
        }
        /// <summary>
        /// Get all leaves 
        /// </summary>
        /// <returns>ResonseDto object which contains a list of leaves in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ResponseDto> GetAllLeaves(int page = 1, int pageSize = 10)
        {
            var _response = new ResponseDto();
            try
            {
                // Start with base query
                IQueryable<Leave> query = _dbContext.Leaves
                    .Include(l => l.LeavesType)
                    .Include(l => l.Employee);

                // 🔹 Apply permission filter
                //var isAdmin = User.IsInRole("Admin"); // Or however you check role
                //if (!isAdmin)
                //{
                //    query = query.Where(l => l.Employee.IsActive);
                //    // or add your own rule: only show leaves of the logged-in employee
                //}

                // 🔹 Count total for pagination
                var totalCount = await query.CountAsync();

                // 🔹 Fetch paged data
                var leaves = await query
                    .OrderByDescending(l => l.StartDate) // Sort newest first
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 🔹 Map to DTO
                var responseData = _mapper.Map<List<ViewLeaveDto>>(leaves);

                // 🔹 Wrap with pagination metadata
                _response.Result = new
                {
                    Data = responseData,
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
                _response.Result = null;
                _response.Message = ex.Message;
                _response.IsSuccess = false;
                return _response;
            }
        }

        /// <summary>
        /// Get single leave based on supplied Id
        /// </summary>
        /// <param name="id">leave id</param>
        /// <returns>ResponseDto object contains Leave in Result prop</returns>
        [HttpGet("{id}")]
        public ResponseDto Leaves(int id)
        {
            ResponseDto response = new ResponseDto();
            var leave = _dbContext.Leaves.AsNoTracking().Include(l => l.LeavesType).Include(e => e.Employee).FirstOrDefault(x => x.Id == id);

            if (leave == null)
            {
                response.Message = "There is no leave";
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }
            else
            {
                ViewLeaveDto LeaveDto = _mapper.Map<ViewLeaveDto>(leave);
                response.Result = LeaveDto;
                response.IsSuccess = true;
                response.Message = "";
                return response;
            }

        }
        [HttpPost]
        public async Task<ResponseDto> Leaves(CreateLeaveDto createLeaveDto)
        {
            var response = new ResponseDto();
           
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Message = "Validation failed";
                response.Result = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }
            // Validate the employee exists and grab their department (no full-entity load).
            var emp = await _dbContext.Employees
                .Where(e => e.Id == createLeaveDto.EmployeeId)
                .Select(e => new { e.Id, e.DepartmentId })
                .FirstOrDefaultAsync();
            if (emp is null)
            {
                response.IsSuccess = false;
                response.Message = "Selected employee was not found.";
                response.Result = null;
                return response;
            }

            // Manager is the employee's DEPARTMENT manager (not a per-employee manager).
            var deptManagerId = await _dbContext.Departments
                .Where(d => d.Id == emp.DepartmentId)
                .Select(d => d.ManagerId)
                .FirstOrDefaultAsync();

            // Authorize: an Admin, the employee themselves, or the manager of the
            // employee's department (lets a manager file leave for their team members).
            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var caller);
            var allowed = isAdmin || caller == emp.Id || (deptManagerId.HasValue && deptManagerId.Value == caller);
            if (!allowed)
            {
                response.IsSuccess = false;
                response.Message = "You are not authorized to create a leave for this employee.";
                response.Result = null;
                return response;
            }

            // ---- Business validation ------------------------------------------------
            if (!createLeaveDto.EndDate.HasValue)
            {
                response.IsSuccess = false;
                response.Message = "End date is required.";
                return response;
            }
            if (createLeaveDto.EndDate.Value.Date < createLeaveDto.StartDate.Date)
            {
                response.IsSuccess = false;
                response.Message = "End date cannot be before the start date.";
                return response;
            }
            if (createLeaveDto.IsHalfDay && createLeaveDto.StartDate.Date != createLeaveDto.EndDate.Value.Date)
            {
                response.IsSuccess = false;
                response.Message = "A half-day leave must start and end on the same day.";
                return response;
            }

            // Reject overlapping requests (anything not already rejected counts).
            var overlaps = await _dbContext.Leaves.AnyAsync(l =>
                l.EmployeeId == createLeaveDto.EmployeeId
                && l.Status.ToUpper() != "REJECTED"
                && l.EndDate.HasValue
                && createLeaveDto.StartDate.Date <= l.EndDate.Value.Date
                && l.StartDate.Date <= createLeaveDto.EndDate.Value.Date);
            if (overlaps)
            {
                response.IsSuccess = false;
                response.Message = "This employee already has a leave that overlaps these dates.";
                return response;
            }

            // Enforce the leave-type entitlement (committed + pending must not exceed MaxDays this year).
            var leaveType = await _dbContext.LeavesTypes
                .Where(t => t.Id == createLeaveDto.LeavesTypeId)
                .Select(t => new { t.Name, t.MaxDays })
                .FirstOrDefaultAsync();
            if (leaveType is null)
            {
                response.IsSuccess = false;
                response.Message = "Selected leave type was not found.";
                return response;
            }

            var year = createLeaveDto.StartDate.Year;
            var committed = (await _dbContext.Leaves
                .Where(l => l.EmployeeId == createLeaveDto.EmployeeId
                            && l.LeavesTypeId == createLeaveDto.LeavesTypeId
                            && l.Status.ToUpper() != "REJECTED"
                            && l.EndDate.HasValue
                            && l.StartDate.Year == year)
                .Select(l => new { l.StartDate, l.EndDate, l.IsHalfDay })
                .ToListAsync())
                .Sum(l => Services.LeaveCalculations.EffectiveDays(l.StartDate, l.EndDate!.Value, l.IsHalfDay));

            var requestedDays = Services.LeaveCalculations.EffectiveDays(
                createLeaveDto.StartDate, createLeaveDto.EndDate.Value, createLeaveDto.IsHalfDay);

            if (committed + requestedDays > leaveType.MaxDays)
            {
                var remaining = Services.LeaveCalculations.Remaining((decimal)leaveType.MaxDays, committed);
                response.IsSuccess = false;
                response.Message = $"Insufficient {leaveType.Name} balance: {remaining} day(s) remaining, but {requestedDays} requested.";
                return response;
            }
            // -------------------------------------------------------------------------

            Leave leaveModel = _mapper.Map<Leave>(createLeaveDto);
            leaveModel.IsHalfDay = createLeaveDto.IsHalfDay;
            // An admin-created leave is approved on the spot; everyone else's starts pending.
            leaveModel.Status = isAdmin ? Emp.Api.Utility.SD.Approved : Emp.Api.Utility.SD.Pending;
            if (isAdmin)
            {
                leaveModel.DecidedById = caller;
                leaveModel.DecidedAt = DateTime.Now;
                leaveModel.DecisionNote = "Auto-approved (created by admin).";
            }
            // Route to the nearest manager up the department tree who isn't the requester
            // themselves. A top-level manager's own request resolves to null (Admin approval).
            leaveModel.ManagerId = await ResolveApproverAsync(emp.DepartmentId, emp.Id);

            leaveModel.Note ??= string.Empty;

            _dbContext.Leaves.Add(leaveModel);
            await _dbContext.SaveChangesAsync();
            if (createLeaveDto.Attachment != null)
            {
                //savedPath = await _fileService.SaveFileAsync(createLeaveDto.Attachment, "leaves");
                string fileName = leaveModel.Id + Path.GetExtension(createLeaveDto.Attachment.FileName);
                string filePath = @"wwwroot\uploads\leaves\" + fileName;
                var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                {
                    createLeaveDto?.Attachment?.CopyTo(fileStream);
                }
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                leaveModel.FilePath = baseUrl + "/uploads/leaves/" + fileName;
            }
            else
            {
                leaveModel.FilePath = "https://placeholde.co/600x400";
            }
            _dbContext.Update(leaveModel);
            await _dbContext.SaveChangesAsync();

            // Notify the approver that a request is waiting (only for pending requests with a manager).
            if (!isAdmin && leaveModel.ManagerId.HasValue)
            {
                var requesterName = await _dbContext.Employees
                    .Where(e => e.Id == leaveModel.EmployeeId)
                    .Select(e => e.Name)
                    .FirstOrDefaultAsync();
                _dbContext.Notifications.Add(new Notification
                {
                    RecipientEmployeeId = leaveModel.ManagerId.Value,
                    Message = $"{requesterName} requested {leaveType.Name} leave awaiting your approval.",
                    Url = "/Leaves/TeamLeaves",
                });
                await _dbContext.SaveChangesAsync();
            }

            response.IsSuccess = true;
            response.Message = "Leave created successfully";
            // Return a flat projection — never the tracked entity (its navigation graph causes serialization cycles).
            response.Result = new
            {
                leaveModel.Id,
                leaveModel.EmployeeId,
                leaveModel.LeavesTypeId,
                leaveModel.ManagerId,
                leaveModel.Status,
                leaveModel.StartDate,
                leaveModel.EndDate,
                leaveModel.FilePath
            };
            return response;
        }

        /// <summary>
        /// Finds who should approve a leave by climbing the department tree from the
        /// employee's department upward. Returns the manager of the nearest department
        /// whose manager is not the requester. Returns null at the top of the tree (or
        /// when no manager exists), meaning the request falls to Admin approval.
        /// </summary>
        private async Task<int?> ResolveApproverAsync(int departmentId, int employeeId)
        {
            int? currentDeptId = departmentId;
            var visited = new HashSet<int>();

            while (currentDeptId.HasValue && visited.Add(currentDeptId.Value))
            {
                var dept = await _dbContext.Departments
                    .Where(d => d.Id == currentDeptId.Value)
                    .Select(d => new { d.ManagerId, d.ParentDepartmentId })
                    .FirstOrDefaultAsync();

                if (dept is null)
                {
                    break;
                }

                if (dept.ManagerId.HasValue && dept.ManagerId.Value != employeeId)
                {
                    return dept.ManagerId.Value;
                }

                currentDeptId = dept.ParentDepartmentId;
            }

            return null;
        }

        /// <summary>
        /// this function is used to update leave request
        /// </summary>
        /// <param name="id">respresnt leave id</param>
        /// <param name="updateLeaveDto">leave DTO object </param>
        /// <returns>ResponseDto object </returns>
        //[HttpPut("{id}")]
        //public async Task<ResponseDto> Leaves(int id, [FromBody] UpdateLeaveDto updateLeaveDto)
        //{
        //    ResponseDto response = new ResponseDto();
        //    var LeaveModel = _dbContext.Leaves.AsNoTracking().FirstOrDefault(x => x.Id == id);
        //    if (LeaveModel != null)
        //    {
        //        _dbContext.Update(_mapper.Map<Leave>(updateLeaveDto));
        //        await _dbContext.SaveChangesAsync();
        //        response.IsSuccess = true;
        //        response.Result = LeaveModel;
        //        response.Message = "Leave updated successfully";
        //        return response;
        //    }
        //    else
        //    {
        //        response.Message = leave_not_found;
        //        response.Result = null;
        //        response.IsSuccess = false;
        //        return response;
        //    }
        //}
        //[HttpPatch]
        //public async Task<ResponseDto> LeaveAction(int leaveId, string status, string message)
        //{
        //    var response = new ResponseDto();

        //    var targetLeave = await _dbContext.Leaves.FirstOrDefaultAsync(x => x.Id == leaveId);
        //    if (targetLeave == null)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = "Leave not found";
        //        return response;
        //    }

        //    targetLeave.Status = status;
        //    targetLeave.Note = message;
        //    targetLeave.UpdatedAt = DateTime.Now;
        //    targetLeave.IsModified = true;
        //    //targetLeave.ApprovedBy = managerId;

        //    await _dbContext.SaveChangesAsync();

        //    response.IsSuccess = true;
        //    response.Message = "Leave updated successfully";
        //    response.Result = true;
        //    //{
        //    //    Id = targetLeave.Id,
        //    //    Status = targetLeave.Status,
        //    //    Note = targetLeave.Note,
        //    //    UpdatedAt = targetLeave.UpdatedAt,
        //    //    IsModified = targetLeave.IsModified,
        //    //    ApprovedBy = targetLeave.ApprovedBy
        //    //};

        //    return response;
        //}

        /// <summary>
        /// This function delete a leave based on it's id
        /// </summary>
        /// <param name="id">leave id parameter</param>
        /// <returns>should return ResponseDto object which contains deleted leave </returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> DeleteLeave(int id)
        {
            ResponseDto response = new ResponseDto();
            var targetLeave = await _dbContext.Leaves.FindAsync(id);
            if (targetLeave != null)
            {
                _dbContext.Leaves.Remove(targetLeave);
                await _dbContext.SaveChangesAsync();
                response.IsSuccess = true;
                response.Result = new { targetLeave.Id };
                response.Message = "Leave deleted successfully";
                return response;
            }
            else
            {
                response.Result = null;
                response.Message = leave_not_found;
                response.IsSuccess = false;
                return response;
            }
        }
        /// <summary>
        /// this function will take an action on leave => to update it's status 
        /// </summary>
        /// <param name="leaveId">leave Id which we will update</param>
        /// <param name="status">current status</param>
        /// <param name="message">message or note with action</param>
        /// <returns>ResponseDto which contain result and message</returns>
        //[HttpPatch]
        //public async Task<ResponseDto> LeaveAction(int leaveId, string status, string message)
        //{
        //    //var response = new ResponseDto();
        //    ResponseDto response = new ResponseDto();
        //    var targetLeave = await _dbContext.Leaves.FirstOrDefaultAsync(x => x.Id == leaveId);
        //    if (targetLeave == null)
        //    {
        //        response.Result = null;
        //        response.IsSuccess = false;
        //        response.Message = leave_not_found;
        //        return response;
        //    }
        //    else
        //    {
        //        targetLeave.Status = status;
        //        targetLeave.UpdatedAt = DateTime.Now;
        //        targetLeave.IsModified = true;
        //        targetLeave.Note = message;
        //        await _dbContext.SaveChangesAsync();
        //        response.Message = "Leave updated successfully";
        //        response.IsSuccess = true;
        //        response.Result = targetLeave;
        //        return response;

        //        //must add the manager who accept the leave
        //    }
        //}
        /// <summary>
        /// this function will get leaves based on employee
        /// </summary>
        /// <param name="employeeId">employee Id</param>
        /// <returns>ResponseDto which contains leaves list in Result object</returns>
        [HttpGet]
        [Route("get-leaves-by-employeeId/{employeeId}")]
        public async Task<ResponseDto> GetLeavesByEmployeeId(int employeeId)
        {
            var response = new ResponseDto();

            // A user may only read their own leaves; admins may read anyone's.
            if (!User.IsInRole("Admin"))
            {
                int.TryParse(User.FindFirst("EmployeeId")?.Value, out var caller);
                if (caller != employeeId)
                {
                    response.IsSuccess = false;
                    response.Message = "You are not authorized to view these leaves.";
                    return response;
                }
            }

            try
            {
                var leaves = await _dbContext.Leaves
                    .Where(e => e.EmployeeId == employeeId)
                    .Include(e=>e.Employee)
                    .Include(e=>e.LeavesType)
                    .ToListAsync();

                if (leaves.Any())
                {
                    response.Result = _mapper.Map<List<ViewLeaveDto>>(leaves);
                    response.IsSuccess = true;
                    response.Message = "Leaves retrieved successfully";
                }
                else
                {
                    response.Result = new List<Leave>();
                    response.IsSuccess = true;
                    response.Message = "No leaves found";
                }
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.IsSuccess = false;
                response.Message = $"Error: {ex.Message}";
            }
            return response;
        }
        /// <summary>
        /// This function is for update leave status 
        /// </summary>
        /// <param name="updateLeaveDto">ResponseDto object</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ResponseDto> LeaveAction(UpdateLeaveDto updateLeaveDto)
        {
            var response = new ResponseDto();
            try
            {
                var targetLeave = await _dbContext.Leaves.FirstOrDefaultAsync(x => x.Id == updateLeaveDto.Id);
                if (targetLeave == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Leave not found";
                    return response;
                }
                //What’s happening

                //When you query with FirstOrDefaultAsync, EF is already tracking targetLeave.

                //You set targetLeave.Status = updateLeaveDto.Status;.

                //EF sees the property change → and when you call SaveChangesAsync(), it will persist it.

                //But by calling _dbContext.Update(targetLeave) you override the entity state, telling EF “all fields are modified”.
                //            If some fields are not in the DTO, EF may overwrite them with null/default.
                // Apply updates
                // Update only fields you want to allow from client
                targetLeave.Status = updateLeaveDto.Status;
                targetLeave.Note = updateLeaveDto.Note;
                //targetLeave.StartDate = updateLeaveDto.StartDate;

                targetLeave.UpdatedAt = DateTime.Now;
                targetLeave.IsModified = true;

                var saved = await _dbContext.SaveChangesAsync();

                response.IsSuccess = saved > 0;
                response.Message = saved > 0 ? "Leave updated successfully" : "No changes saved";
                response.Result = saved;

                return response;
            }
            catch (Exception ex)
            {
                response.Message=ex.Message;
                response.Result=null;
                response.IsSuccess = false;
                return response;
            }

        }

        /// <summary>
        /// Approve or reject a leave. Allowed for an Admin or the leave's own (department) manager.
        /// </summary>
        [Authorize]
        [HttpPatch("{id}/decision")]
        public async Task<ResponseDto> Decide(int id, string status, string? note = null)
        {
            var response = new ResponseDto();

            if (status != "Approved" && status != "Rejected")
            {
                response.IsSuccess = false;
                response.Message = "Status must be 'Approved' or 'Rejected'.";
                return response;
            }

            var leave = await _dbContext.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
            {
                response.IsSuccess = false;
                response.Message = "Leave not found.";
                return response;
            }

            // A rejection must carry a reason for the audit trail.
            if (status == "Rejected" && string.IsNullOrWhiteSpace(note))
            {
                response.IsSuccess = false;
                response.Message = "A reason is required when rejecting a request.";
                return response;
            }

            // Authorize: Admin, or the manager this leave is routed to.
            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var callerEmployeeId);
            if (!isAdmin && (leave.ManagerId is null || leave.ManagerId != callerEmployeeId))
            {
                response.IsSuccess = false;
                response.Message = "You are not authorized to act on this request.";
                return response;
            }

            // Store status using the canonical (uppercase) constants so balance/overlap
            // queries are consistent regardless of how the caller cased it.
            leave.Status = status == "Approved" ? Emp.Api.Utility.SD.Approved : Emp.Api.Utility.SD.Rejected;
            leave.DecidedById = callerEmployeeId == 0 ? (int?)null : callerEmployeeId;
            leave.DecidedAt = DateTime.Now;
            leave.DecisionNote = note;
            leave.UpdatedAt = DateTime.Now;
            leave.IsModified = true;
            await _dbContext.SaveChangesAsync();

            // Tell the employee the outcome.
            var typeName = await _dbContext.LeavesTypes
                .Where(t => t.Id == leave.LeavesTypeId).Select(t => t.Name).FirstOrDefaultAsync();
            _dbContext.Notifications.Add(new Notification
            {
                RecipientEmployeeId = leave.EmployeeId,
                Message = $"Your {typeName} leave request was {status.ToLower()}."
                          + (string.IsNullOrWhiteSpace(note) ? "" : $" Note: {note}"),
                Url = "/Leaves/EmployeeLeaves",
            });
            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = new { leave.Id, leave.Status };
            response.Message = $"Leave {status.ToLower()}.";
            return response;
        }

        [Authorize]
        [HttpGet("get-leaves-by-managerId/{managerId}")]
        public async Task<ResponseDto> GetLeavesByManagerAsync(int managerId)
        {
            var response = new ResponseDto();

            // A manager may only read the leaves routed to them; admins may read any manager's.
            if (!User.IsInRole("Admin"))
            {
                int.TryParse(User.FindFirst("EmployeeId")?.Value, out var caller);
                if (caller != managerId)
                {
                    response.IsSuccess = false;
                    response.Message = "You are not authorized to view these leaves.";
                    return response;
                }
            }

            try
            {
                var leavesResponse = await _dbContext.Leaves
                    .Where(l => l.ManagerId == managerId)
                    .Include(l => l.Employee)
                    .Include(l => l.LeavesType)
                    .Select(l => new
                    {
                        l.Id,
                        l.StartDate,
                        l.EndDate,
                        l.Status,
                        l.Note,
                        EmployeeName = l.Employee.Name,
                        LeaveType = l.LeavesType.Name
                    })
                    .ToListAsync();

                response.IsSuccess = true;
                response.Result = leavesResponse;
                response.Message = "";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Returns leave balance per leave type for an employee:
        /// entitlement (MaxDays) minus approved days taken in the current year.
        /// </summary>
        [HttpGet("balance/{employeeId}")]
        public async Task<ResponseDto> GetLeaveBalance(int employeeId)
        {
            var response = new ResponseDto();
            try
            {
                var year = DateTime.UtcNow.Year;
                var leaveTypes = await _dbContext.LeavesTypes
                    .Where(t => t.IsActive)
                    .ToListAsync();

                var approved = await _dbContext.Leaves
                    .Where(l => l.EmployeeId == employeeId
                                && l.Status.ToUpper() == "APPROVED"
                                && l.EndDate.HasValue
                                && l.StartDate.Year == year)
                    .Select(l => new { l.LeavesTypeId, l.StartDate, l.EndDate, l.IsHalfDay })
                    .ToListAsync();

                var balances = leaveTypes.Select(t =>
                {
                    var taken = approved
                        .Where(a => a.LeavesTypeId == t.Id)
                        .Sum(a => Services.LeaveCalculations.EffectiveDays(a.StartDate, a.EndDate!.Value, a.IsHalfDay));
                    return new
                    {
                        LeaveTypeId = t.Id,
                        LeaveType = t.Name,
                        Entitlement = (decimal)t.MaxDays,
                        Taken = taken,
                        Remaining = Services.LeaveCalculations.Remaining((decimal)t.MaxDays, taken)
                    };
                }).ToList();

                response.Result = balances;
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

        [HttpPost("TestUpload")]
        public async Task<IActionResult> TestUpload(IFormFile file)
        {
            var path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "leaves", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok("Saved to: " + path);
        }
    }
}


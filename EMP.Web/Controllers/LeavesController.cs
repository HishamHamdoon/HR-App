using Emp.Api.Dtos.Leave;
using Emp.Api.Models;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using EMP.Web.Models;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EMP.Web.Controllers
{
    public class LeavesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly ILeaveService _leavService;
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;
        private readonly ICountryService _countryService;
        private readonly ISectionService _sectionService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ILeavesTypeService _leaveTypeService;
        private readonly ISetupService _setupService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public LeavesController(ILogger<HomeController> logger, HttpClient httpClient,
            IEmployeeService employeeService,
            ICountryService countryService,
            IDepartmentService departmentService,
            ISectionService sectionService,
            IJobTitleService jobTitleService,
            ILeaveService leaveService,
            ILeavesTypeService leavesTypeService,
            ISetupService setupService,
            IWebHostEnvironment hostEnvironment
           )
        {
            _httpClient = httpClient;
            _leavService = leaveService;
            _departmentService = departmentService;
            _countryService = countryService;
            _logger = logger;
            _sectionService = sectionService;
            _jobTitleService = jobTitleService;
            _employeeService = employeeService;
            _leaveTypeService=leavesTypeService;
            _setupService = setupService;
            _hostEnvironment = hostEnvironment;
        }
        // The full leaves list is admin-only. A manager is sent to their team's leaves,
        // a regular employee to their own.
        [Authorize]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 1000)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("EmployeeLeaves");
            }

            ResponseDto response = await _leavService.GetLeavesAsync(page, pageSize);
            var leavesList = new PagedLeavesVM();

            if (response.IsSuccess && response.Result is not null)
            {
                var pagedResult = JsonConvert.DeserializeObject<PagedApiResponse<Emp.Web.Dtos.ViewLeaveDto>>(response.Result.ToString());

                leavesList = new PagedLeavesVM
                {
                    Leaves = pagedResult.Data,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount
                };

                return View(leavesList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View(new PagedLeavesVM()); // return empty VM to avoid null issues
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int leaveId)
        {
            ResponseDto response = await _leavService.GetLeaveAsync(leaveId);
            if (response is not null && response.IsSuccess)
            {
                var leave = JsonConvert.DeserializeObject<Emp.Web.Dtos.ViewLeaveDto>(Convert.ToString(response.Result));
                return View(leave);
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int leaveId)
        {
            ResponseDto response = await _leavService.GetLeaveAsync(leaveId);
            if (response is not null && response.IsSuccess)
            {
                var leave = JsonConvert.DeserializeObject<Emp.Web.Dtos.UpdateLeaveDto>(Convert.ToString(response.Result));
                return View(leave);
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]  // POST instead of PUT
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Emp.Web.Dtos.UpdateLeaveDto leaveDto)
        {
            var response = await _leavService.EditLeaveAsync(leaveDto);
            if (response.IsSuccess)
            {
                TempData["Success"] = "Leave updated Successfully";
                return RedirectToAction("Index");
            }
            return View(leaveDto);
        }
        /// <summary>
        /// In case employee => if he is logged in 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var dto = new Emp.Web.Dtos.CreateLeaveDto();
            var isManager = User.HasClaim("IsManager", "true");
            ViewBag.IsManager = isManager;

            // Non-admins default to themselves. A manager may instead pick a team member;
            // a plain employee is locked to their own id (their manager is shown read-only).
            if (!User.IsInRole("Admin"))
            {
                var empIdClaim = User.FindFirst("EmployeeId")?.Value;
                if (!string.IsNullOrEmpty(empIdClaim) && int.TryParse(empIdClaim, out var empId))
                {
                    dto.EmployeeId = empId;
                    ViewBag.EmployeeId = empId;
                    ViewBag.ManagerName = await GetDepartmentManagerNameAsync(empId);
                }
            }

            await PopulateLeaveDropdownsAsync(dto);
            return View(dto);
        }

        /// <summary>Resolves the department-manager name for an employee (for read-only display).</summary>
        private async Task<string> GetDepartmentManagerNameAsync(int employeeId)
        {
            try
            {
                var empResp = await _employeeService.GetEmployeeAsync(employeeId);
                if (empResp?.IsSuccess == true && empResp.Result is not null)
                {
                    var emp = JsonConvert.DeserializeObject<EMP.Web.Views.ViewModels.EmployeeVM>(Convert.ToString(empResp.Result));
                    if (emp is not null && emp.DepartmentId > 0)
                    {
                        var deptResp = await _departmentService.GetDepartmentAsync(emp.DepartmentId);
                        if (deptResp?.IsSuccess == true && deptResp.Result is not null)
                        {
                            var dept = JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentDto>(Convert.ToString(deptResp.Result));
                            if (!string.IsNullOrEmpty(dept?.ManagerName))
                                return dept.ManagerName;
                        }
                    }
                }
            }
            catch { /* fall through */ }
            return "Not assigned";
        }

        private async Task PopulateLeaveDropdownsAsync(Emp.Web.Dtos.CreateLeaveDto dto)
        {
            dto.LeavesTypes = await LoadDropdownAsync<LeavesType>(_setupService.GetLeaveTypesList);

            // Admin chooses from all employees; a manager from their team (plus themselves);
            // a plain employee gets no employee dropdown (locked to self).
            if (User.IsInRole("Admin"))
            {
                dto.Employees = await LoadDropdownAsync<Employee>(_setupService.GetEmployeesList);
            }
            else if (User.HasClaim("IsManager", "true")
                     && int.TryParse(User.FindFirst("EmployeeId")?.Value, out var managerId))
            {
                dto.Employees = await GetTeamWithSelfAsync(managerId);
            }
            else
            {
                dto.Employees = new List<Employee>();
            }
        }

        /// <summary>The manager (labelled "You") followed by their team members, for the leave dropdown.</summary>
        private async Task<List<Employee>> GetTeamWithSelfAsync(int managerId)
        {
            var list = new List<Employee>();

            var selfResp = await _employeeService.GetEmployeeAsync(managerId);
            var selfName = "You";
            if (selfResp?.IsSuccess == true && selfResp.Result is not null)
            {
                var self = JsonConvert.DeserializeObject<EMP.Web.Views.ViewModels.EmployeeVM>(Convert.ToString(selfResp.Result));
                if (!string.IsNullOrEmpty(self?.Name)) selfName = $"{self.Name} (You)";
            }
            list.Add(new Employee { Id = managerId, Name = selfName });

            foreach (var t in await GetTeamMembersAsync(managerId))
            {
                list.Add(new Employee { Id = t.Id, Name = t.Name });
            }
            return list;
        }

        private async Task<List<EMP.Web.Views.ViewModels.EmployeeVM>> GetTeamMembersAsync(int managerId)
        {
            var resp = await _employeeService.GetByManagerAsync(managerId);
            if (resp?.IsSuccess == true && resp.Result is not null)
            {
                return JsonConvert.DeserializeObject<List<EMP.Web.Views.ViewModels.EmployeeVM>>(Convert.ToString(resp.Result))
                       ?? new List<EMP.Web.Views.ViewModels.EmployeeVM>();
            }
            return new List<EMP.Web.Views.ViewModels.EmployeeVM>();
        }

        private static async Task<List<T>> LoadDropdownAsync<T>(Func<Task<ResponseDto>> call)
        {
            var response = await call();
            if (response?.IsSuccess != true || response.Result is null)
            {
                return new List<T>();
            }
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(Convert.ToString(response.Result)) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Emp.Web.Dtos.CreateLeaveDto dto, IFormFile? attachment)
        {
            ViewBag.IsManager = User.HasClaim("IsManager", "true");

            // Non-admins: a manager may submit for themselves or a team member (validated below);
            // a plain employee is always pinned to their own id.
            if (!User.IsInRole("Admin")
                && int.TryParse(User.FindFirst("EmployeeId")?.Value, out var selfId))
            {
                if (User.HasClaim("IsManager", "true"))
                {
                    var allowedIds = new HashSet<int> { selfId };
                    foreach (var t in await GetTeamMembersAsync(selfId)) allowedIds.Add(t.Id);
                    if (!allowedIds.Contains(dto.EmployeeId))
                    {
                        dto.EmployeeId = selfId; // ignore tampered / out-of-team ids
                    }
                }
                else
                {
                    dto.EmployeeId = selfId;
                }
                ViewBag.EmployeeId = dto.EmployeeId;
            }

            if (!ModelState.IsValid)
            {
                await PopulateLeaveDropdownsAsync(dto);
                return View(dto);
            }

            var response = await _leavService.CreateLeaveAsync(dto);

            if (response.IsSuccess && response.Result != null)
            {
                TempData["success"] = "Leave created successfully!";
                if (User.IsInRole("Employee"))
                {
                    return RedirectToAction("EmployeeDashboard", "Home");
                }
                return RedirectToAction("Index");
            }

            TempData["error"] = response.Message ?? "Something went wrong!";
            await PopulateLeaveDropdownsAsync(dto);
            return View(dto);
        }
        [Authorize(Roles="Employee")]
        [HttpGet]
        public async Task<IActionResult> EmployeeLeaves()
        {
            // Get employeeId from claims
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || !int.TryParse(employeeIdClaim, out var employeeId))
            {
                TempData["Error"] = "EmployeeId not found in token.";
                return View(new List<Emp.Web.Dtos.ViewLeaveDto>()); // ✅ return correct type
            }

            var response = await _leavService.GetLeavesByEmployeeIdAsync(employeeId);

            if (!response.IsSuccess || response.Result == null)
            {
                TempData["Error"] = response.Message ?? "Failed to load leaves";
                return View(new List<Emp.Web.Dtos.ViewLeaveDto>()); // ✅ return correct type
            }

            var leavesList = JsonConvert.DeserializeObject<List<Emp.Web.Dtos.ViewLeaveDto>>(
                JsonConvert.SerializeObject(response.Result)
            );

            // Always return List<ViewLeaveDto>
            return View(leavesList ?? new List<Emp.Web.Dtos.ViewLeaveDto>());
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int leaveId)
        {
            if (leaveId>0)
            {
                var response = await _leavService.DeleteLeaveAsync(leaveId);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index","Leaves");
                }
            }
            else
            {
                TempData["Success"] = "Something went wrong!";
                return RedirectToAction("Index", "Home");
            }
            return View("Index");
        }

        // Manager (or admin) approves/rejects a request. The API authorizes the leave's manager.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Approve(int leaveId, string returnUrl = null)
            => await DecideAndRedirect(leaveId, "Approved", null, returnUrl);

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(int leaveId, string note = null, string returnUrl = null)
            => await DecideAndRedirect(leaveId, "Rejected", note, returnUrl);

        private async Task<IActionResult> DecideAndRedirect(int leaveId, string status, string note, string returnUrl)
        {
            if (leaveId > 0)
            {
                var response = await _leavService.DecideLeaveAsync(leaveId, status, note);
                TempData[response?.IsSuccess == true ? "success" : "error"] =
                    response?.IsSuccess == true ? (response.Message ?? $"Leave {status.ToLower()}.")
                                                : (response?.Message ?? "Failed to update the request.");
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return User.IsInRole("Admin")
                ? RedirectToAction("Index", "Leaves")
                : RedirectToAction("EmployeeDashboard", "Home");
        }
        // Leaves routed to the logged-in user as a department manager. A non-manager simply
        // sees an empty list. Admins can review any manager via the full Index.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TeamLeaves()
        {
            if (!int.TryParse(User.FindFirst("EmployeeId")?.Value, out var managerId) || managerId <= 0)
            {
                return View("AdminDashboard", new List<LeaveByManagerDto>());
            }

            var response = await _leavService.GetLeavesByManagerAsync(managerId);
            if (response.IsSuccess && response.Result != null)
            {
                var leaves = JsonConvert.DeserializeObject<List<LeaveByManagerDto>>(response.Result.ToString());
                return View("AdminDashboard", leaves ?? new List<LeaveByManagerDto>());
            }

            return View("AdminDashboard", new List<LeaveByManagerDto>());
        }

        private class LeavesTypeVM
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        private class EmployeesVM
        {
            public int Id { get; set; }
            public string Name { get; set; }
            List<Employee> Employees { get; set; } = new List<Employee>();
            List<LeavesType> LeaveTypes{ get; set; } = new List<LeavesType>();
        }
        

    }
}

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
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
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
            var role = User.FindFirst(ClaimTypes.Role)?.Value;//.Select(c => c.Value.ToUpper()).ToList();
            var admin = "Admin";
           

            //if (!roles.Contains(admin))
            //{
            if (User.IsInRole("Employee"))
            {

                // Not admin: force employee id from claim
                var empIdClaim = User.FindFirst("EmployeeId")?.Value;
                if (!string.IsNullOrEmpty(empIdClaim))
                {
                    dto.EmployeeId = int.Parse(empIdClaim);
                    ViewBag.EmployeeId = dto.EmployeeId;
                    var manager = await _employeeService.GetEmployeeAsync(int.Parse(empIdClaim));
                    var managerId = await _employeeService.GetEmployeeAsync(int.Parse(empIdClaim));
                    var managerName = await _employeeService.GetManagerNameAsync(int.Parse(empIdClaim));
                    ViewBag.ManagerId= managerName.Result;
                    //dto.ManagerId=ViewBag.m

                }
            }
            else if(role?.ToString()==admin)
            {
                //// ✅ Get employees (list directly, no ToString/extra deserialize if already List<Employee>)
                //var employeeList = await _setupService.GetEmployeesList();
                //if (employeeList != null && employeeList.Result != null)
                //{
                //    try
                //    {
                //        // If Result is already a JArray or stringified JSON, force it into List<Employee>
                //        var emps = JsonConvert.DeserializeObject<List<Employee>>(employeeList.Result.ToString());
                //        dto.Employees = emps ?? new List<Employee>();
                //    }
                //    catch
                //    {
                //        dto.Employees = new List<Employee>();
                //    }
                //}
            }
            // ✅ Get employees (list directly, no ToString/extra deserialize if already List<Employee>)
            var employeeList = await _setupService.GetEmployeesList();
            if (employeeList != null && employeeList.Result != null)
            {
                try
                {
                    // If Result is already a JArray or stringified JSON, force it into List<Employee>
                    var emps = JsonConvert.DeserializeObject<List<Employee>>(employeeList.Result.ToString());
                    dto.Employees = emps ?? new List<Employee>();
                }
                catch
                {
                    dto.Employees = new List<Employee>();
                }
            }

            // ✅ Get leave types
            var leaveTypesList = await _setupService.GetLeaveTypesList();
            if (leaveTypesList != null && leaveTypesList.Result != null)
            {
                try
                {
                    var leavesTypes = JsonConvert.DeserializeObject<List<LeavesType>>(leaveTypesList.Result.ToString());
                    dto.LeavesTypes = leavesTypes ?? new List<LeavesType>();
                }
                catch
                {
                    dto.LeavesTypes = new List<LeavesType>();
                }
            }

            return View(dto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Emp.Web.Dtos.CreateLeaveDto dto,IFormFile? attachment)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _employeeService.GetEmployeesAsync(1,10);
                ViewBag.LeaveTypes = await _leaveTypeService.GetLeavesTypeAsync();
                return View(dto);
            }
            

            var response = await _leavService.CreateLeaveAsync(dto);

            if (response.IsSuccess && response.Result!=null)
            {
                TempData["success"] = "Leave created successfully!";
                if (User.IsInRole("Employee"))
                {
                    return RedirectToAction("EmployeeDashboard", "Home");
                }
                return RedirectToAction("Index");
            }

            TempData["error"] = response.Message ?? "Something went wrong!";
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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            if (User.IsInRole("Admin"))
            {
                var managerId = int.Parse(User.FindFirst("EmployeeId")?.Value);//1013
                if (managerId > 0)
                {
                    var response = await _leavService.GetLeavesByManagerAsync(1013);
                    if (response.IsSuccess && response.Result != null)
                    {
                        // Deserialize into a list
                        var leaves = JsonConvert.DeserializeObject<List<LeaveByManagerDto>>(response.Result.ToString());

                        // Send the list to a view
                        return View(leaves);
                    }
                    else
                    {
                        return View(new List<LeaveByManagerDto>());
                    }
                }
                
            }
            return View(null);   
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

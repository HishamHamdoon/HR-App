using Emp.Api.Controllers;
using Emp.Api.Dtos.Employee;
using Emp.Api.Dtos.Leave;
using Emp.Api.Models;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using EMP.Web.Models;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using EMP.Web.Views.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EMP.Web.Controllers
{
    public class LeavesTypeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICountryService _countryService;
        private readonly ISectionService _sectionService;
        private readonly IJobTitleService _jobTitleService;
        ILeavesTypeService _leavesTypeService;

        public LeavesTypeController(ILogger<HomeController> logger, HttpClient httpClient,
            IEmployeeService employeeService,
            ICountryService countryService,
            IDepartmentService departmentService,
            ISectionService sectionService,
            IJobTitleService jobTitleService,
            ILeavesTypeService leavesTypeService
           )
        {
            _httpClient = httpClient;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _countryService = countryService;
            _logger = logger;
            _sectionService = sectionService;
            _jobTitleService = jobTitleService;
            _leavesTypeService = leavesTypeService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            ResponseDto response = await _leavesTypeService.GetLeavesTypeAsync();
            //var leavTypesList = new PaggedLeavTypeVM();
            if (response.IsSuccess && response.Result is not null)
            {
                // Deserialize into wrapper
                var leaveTypes = JsonConvert.DeserializeObject<List<LeaveTypesViewDto>>(response.Result.ToString());

                var leavTypesList = new PaggedLeavTypeVM
                {
                    LeaveTypes = leaveTypes,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = leaveTypes.Count
                };
                return View(leavTypesList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View(new List<LeaveTypesViewDto>());
            }
        }


        public async Task<IActionResult> Index1()
        {
            var response = await _httpClient.GetAsync("https://localhost:7031/api/Employee");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("API returned 401 Unauthorized.");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(countries); // ? This matches what the view expects
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ResponseDto response = await _leavesTypeService.GetLeavesTypeAsync(id);
            if (response is not null && response.IsSuccess)
            {
                var leaveType= JsonConvert.DeserializeObject<LeaveTypesViewDto>(Convert.ToString(response.Result));
                return View(leaveType);
            }
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            //ResponseDto response = await _employeeService.GetEmployeeAsync(Id);
            //Console.WriteLine(response.Result);
            if (id>0)
            {
                var deleteResponse = await _leavesTypeService.DeleteLeavesTypeAsync(id);
                if (deleteResponse.IsSuccess)
                {
                    TempData["success"] = deleteResponse.Message;
                    return RedirectToAction(nameof(Index));
                }
                TempData["error"] = deleteResponse.Message;
                return View(nameof(Index));
            }
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Create(Emp.Web.Dtos.CreateLeaveTypesDto createLeaveTypesDto)
        {
            return View(); // shows the form
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Emp.Web.Dtos.CreateLeaveTypesDto createLeaveTypesDto)
        {
            if (!ModelState.IsValid)
            {
                // Stay on Create view if validation fails
                return View(createLeaveTypesDto);
            }

            var response = await _leavesTypeService.CreateLeavesTypeAsync(createLeaveTypesDto);

            if (response != null && response.IsSuccess)
            {
                TempData["Success"] = response.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = response?.Message ?? "Something went wrong while creating leave type.";
            return View(createLeaveTypesDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ResponseDto response = await _leavesTypeService.GetLeavesTypeAsync(id);
            if (response is not null && response.IsSuccess)
            {
                var leaveType = JsonConvert.DeserializeObject<Emp.Web.Dtos.UpdateLeaveTypesDto>(Convert.ToString(response.Result));
                return View(leaveType);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Emp.Web.Dtos.UpdateLeaveTypesDto updateLeaveTypesDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _leavesTypeService.UpdateLeavesTypeAsync(updateLeaveTypesDto);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            return View();
        }
    }
}

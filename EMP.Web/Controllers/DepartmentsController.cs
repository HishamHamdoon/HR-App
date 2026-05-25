using DocumentFormat.OpenXml.Bibliography;
using Emp.Api.Controllers;
using Emp.Api.Dtos.Department;
using Emp.Api.Dtos.Employee;
using Emp.Api.Models;
using Emp.Web.Models.Dtos;
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
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class DepartmentsController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICountryService _countryService;
        private readonly ISectionService _sectionService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ISetupService _setupService;

        public DepartmentsController(ILogger<HomeController> logger, HttpClient httpClient,
            IEmployeeService employeeService,
            ICountryService countryService,
            IDepartmentService departmentService,
            ISectionService sectionService,
            IJobTitleService jobTitleService
           )
        {
            _httpClient = httpClient;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _countryService = countryService;
            _logger = logger;
            _sectionService = sectionService;
            _jobTitleService = jobTitleService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            ResponseDto response = await _departmentService.GetDepartmentsAsync();
            if (response.IsSuccess && response.Result is not null)
            {
                // Deserialize the response into a paged response of EmployeeVM
                var DepartmentList = JsonConvert.DeserializeObject<List<Emp.Web.Dtos.DepartmentDto>>(response.Result.ToString());
                return View(DepartmentList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View(); // return empty VM to avoid null issues
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //var employees = await _setupService.GetEmployeesList();
            //if (employees.Result != null)
            //{
            //    try
            //    {
            //        // Deserialize into paged wrapper
            //        var employeePaged = JsonConvert.DeserializeObject<PagedApiResponse<Employee>>(Convert.ToString(employees.Result));
            //        //dto.Employees = employeePaged?.Data ?? new List<Employee>();
            //        ViewBag.Employees = employeePaged?.Data ?? new List<Employee>();
            //    }
            //    catch (Exception ex)
            //    {
            //        TempData["error"] = $"Failed to load employees: {ex.Message}";
            //        //dto.Employees = new List<Employee>();
            //    }
            //}

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Emp.Web.Dtos.DepartmentCreateDto departmentDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _departmentService.CreateDepartmentsAsync(departmentDto);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = response.Message;
                    return View();
                }
            }
            else
            {
                return View(departmentDto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ResponseDto response = await _departmentService.GetDepartmentAsync(id);
            if (response is not null && response.IsSuccess)
            {
                var department= JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentDto>(Convert.ToString(response.Result));
                return View(department);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewEmployee(EmployeeFormViewModel employeeCreateDto)
        {
            if (ModelState.IsValid)
            {
                Emp.Web.Models.Dtos.EmployeeCreateDto employee = new()
                {
                    Email = employeeCreateDto.Employee.Email,
                    BirthDate = employeeCreateDto.Employee.BirthDate,
                    Address = employeeCreateDto.Employee.Address,
                    CountryId = employeeCreateDto.Employee.CountryId,
                    DepartmentId = employeeCreateDto.Employee.DepartmentId,
                    HireDate = employeeCreateDto.Employee.HireDate,
                    IsActive = employeeCreateDto.Employee.IsActive,
                    JobTitleId = employeeCreateDto.Employee.JobTitleId,
                    LeavingDate = employeeCreateDto.Employee.LeavingDate,
                    ManagerId = employeeCreateDto.Employee.ManagerId,
                    Name = employeeCreateDto.Employee.Name,
                    Phone = employeeCreateDto.Employee.Phone
                };
                var response = await _employeeService.CreateEmployeeAsync(employee);
                TempData["Success"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                return View(employeeCreateDto);
            }
        }
        public async Task<IActionResult> ActiveDeActiveEmployee(int employeeId)
        {
            if (employeeId > 0)
            {
                var response = await _employeeService.ActiveDeActiveEmployee(employeeId);
                if (response.IsSuccess)
                {
                    TempData["Success"] = "Employee status updated successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Failed to update Employee status";
                    return RedirectToAction("Index");
                }
            }
            else 
            {
                TempData["Errro"] = "Failed to update Employee status";
                return RedirectToAction("Index");
            }

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _departmentService.GetDepartmentAsync(id);
            if (response?.IsSuccess == true && response.Result is not null)
            {
                var dept = JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentCreateDto>(Convert.ToString(response.Result));
                return View(dept);
            }
            TempData["error"] = "Department not found.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Emp.Web.Dtos.DepartmentCreateDto departmentDto)
        {
            if (!ModelState.IsValid)
            {
                return View(departmentDto);
            }
            var response = await _departmentService.UpdateDepartmentAsync(departmentDto);
            if (response?.IsSuccess == true)
            {
                TempData["success"] = response.Message ?? "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = response?.Message ?? "Failed to update department.";
            return View(departmentDto);
        }
        [HttpGet]
        public async Task<IActionResult> GetSectionsByDepartment(int departmentId)
        {
            if (departmentId > 0)
            {
                var response = await _departmentService.GetSectionsByDepartmentAsync(departmentId);
                if (response.IsSuccess)
                {
                    var sections = JsonConvert.DeserializeObject<List<Emp.Api.Models.Department>>(Convert.ToString(response.Result));
                    return Json(sections);
                }
            }
            else
            {
                return RedirectToAction("Index","Employees");
            }
            return RedirectToAction("Index", "Employees");
        }
    }
}

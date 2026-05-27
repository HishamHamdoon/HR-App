using DocumentFormat.OpenXml.Bibliography;
using Emp.Api.Controllers;
using Emp.Api.Dtos;
using Emp.Api.Models;
using Emp.Web.Models.Dtos;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using EMP.Web.Views.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EMP.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class EmployeesController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICountryService _countryService;
        private readonly ISectionService _sectionService;
        private readonly IJobTitleService _jobTitleService;
        private readonly ISetupService _setupService;
        private readonly IAuthService _authService;

        public EmployeesController(ILogger<HomeController> logger, HttpClient httpClient,
            IEmployeeService employeeService,
            ICountryService countryService,
            IDepartmentService departmentService,
            ISectionService sectionService,
            IJobTitleService jobTitleService,
            ISetupService setupService,
            IAuthService authService
           )
        {
            _httpClient = httpClient;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _countryService = countryService;
            _logger = logger;
            _sectionService = sectionService;
            _jobTitleService = jobTitleService;
            _setupService = setupService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 1000)
        {
            // Load the full set; the list uses client-side search + pagination.
            Emp.Web.Models.Dtos.ResponseDto response = await _employeeService.GetEmployeesAsync(page, pageSize);
            //var employeeList = new PagedEmployeesVM();

            if (response.IsSuccess && response.Result is not null)
            {
                // Deserialize the response into a paged response of EmployeeVM
                var pagedResult = JsonConvert.DeserializeObject<PagedApiResponse<EmployeeVM>>(response.Result.ToString());
                // Map it into your view model
                var employeeList = new PagedEmployeesVM
                {
                    Employees = pagedResult.Data,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount
                };

                return View(employeeList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View(new PagedEmployeesVM()); // return empty VM to avoid null issues
            }
        }
        [HttpGet]
        public async Task<IActionResult> EmployeeDetails(int employeeId)
        {
            Emp.Web.Models.Dtos.ResponseDto response = await _employeeService.GetEmployeeAsync(employeeId);
            if (response is not null && response.IsSuccess)
            {
                EmployeeVM employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response?.Result));

                // An employee's manager is the manager of their department.
                if (employee is not null && employee.DepartmentId > 0)
                {
                    var deptResp = await _departmentService.GetDepartmentAsync(employee.DepartmentId);
                    if (deptResp?.IsSuccess == true && deptResp.Result is not null)
                    {
                        var dept = JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentDto>(Convert.ToString(deptResp.Result));
                        employee.Manager = dept?.ManagerName;
                    }
                }

                return View(employee);
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> DeleteEmployeeGet(int employeeId)
        {
            if (employeeId > 0)
            {
                Emp.Web.Models.Dtos.ResponseDto response = await _employeeService.GetEmployeeAsync(employeeId);
                if (response is not null && response.IsSuccess)
                {
                    EmployeeVM employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response.Result));
                    return View(employee);
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int employeeIdToDelete)
        {
            if (employeeIdToDelete <= 0)
                return Json(new { success = false, message = "Invalid employee ID." });

            try
            {
                var deleteResponse = await _employeeService.DeleteEmployeeAsync(employeeIdToDelete);

                if (deleteResponse.IsSuccess)
                {
                    TempData["Success"] = deleteResponse.Message;
                    return Json(new { success = true, message = deleteResponse.Message });
                }
                else
                {
                    TempData["Error"] = deleteResponse.Message;
                    return Json(new { success = false, message = deleteResponse.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var employee = new Emp.Web.Dtos.Auth.RegisterDto
            {
                Employee = new Employee()
            };

            employee.Departments = await LoadDropdownAsync<Emp.Api.Models.Department>(_setupService.GetDepartmentsList);
            employee.Countries = await LoadDropdownAsync<Country>(_setupService.GetCountriesList);
            employee.Sections = await LoadDropdownAsync<Section>(_setupService.GetSectionsList);
            employee.JobTitles = await LoadDropdownAsync<JobTitle>(_setupService.GetJobTitleesList);

            return View(employee);
        }

        private static async Task<List<T>> LoadDropdownAsync<T>(Func<Task<Emp.Web.Models.Dtos.ResponseDto>> call)
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
        public async Task<IActionResult> CreateNewEmployee(Emp.Web.Dtos.Auth.RegisterDto registerDto)
        {
            if (registerDto.Employee is null)
            {
                TempData["error"] = "No employee data was submitted.";
                return RedirectToAction("Create");
            }

            var registerObj = new Emp.Web.Dtos.Auth.RegisterDto
            {
                Email = registerDto.Employee.Email,
                Name = registerDto.Employee.Name,
                Phone = registerDto.Employee.Phone,
                PhoneNumber = registerDto.Employee.Phone,
                Address = registerDto.Employee.Address,
                BirthDate = registerDto.Employee.BirthDate,
                HireDate = registerDto.Employee.HireDate,
                LeavingDate = registerDto.Employee.LeavingDate,
                IsActive = registerDto.Employee.IsActive,
                CountryId = registerDto.Employee.CountryId,
                DepartmentId = registerDto.Employee.DepartmentId,
                JobTitleId = registerDto.Employee.JobTitleId,
                // Password is optional — leave blank and the API applies the default (P@ssw0rd).
                Password = string.IsNullOrWhiteSpace(registerDto.Password) ? string.Empty : registerDto.Password
            };

            var response = await _authService.RegisterAsync(registerObj);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = response.Message ?? "Employee created successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = response?.Message ?? "Failed to create employee.";
            return RedirectToAction("Create");
        }
        public async Task<IActionResult> ActiveDeActiveEmployee(int employeeId)
        {
            if (employeeId > 0)
            {
                var response = await _employeeService.ActiveDeActiveEmployee(employeeId);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
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
        [HttpPost]
        public async Task<IActionResult> CreatePost(EmployeeFormViewModel model)
        {
            // Set JWT token if needed
            var token = HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            }
            var json = System.Text.Json.JsonSerializer.Serialize(model.Employee);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{Emp.Web.Utility.SD.ApiBaseUrl}/api/Employee", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create employee.");

                //model.Departments = await GetDepartmentsAsync();
                //model.Sections = await GetSectionsAsync();
                //model.JobTitles = await GetJobTitlesAsync();

                return View("Create", model);
            }

            TempData["Success"] = "Record added successfully";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int employeeId)
        {
            var response = await _employeeService.GetEmployeeAsync(employeeId);
            if (response?.IsSuccess != true || response.Result is null)
            {
                TempData["error"] = "Employee not found!";
                return RedirectToAction("Index");
            }

            // Deserialize into the view DTO (Manager is a string here, so no nav-object crash).
            var emp = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response.Result))
                      ?? new EmployeeVM();

            var vm = new EmployeeFormViewModel
            {
                Employee = new Employee
                {
                    Id = employeeId,
                    Address = emp.Address,
                    Email = emp.Email,
                    BirthDate = emp.BirthDate,
                    CountryId = emp.CountryId,
                    DepartmentId = emp.DepartmentId,
                    HireDate = emp.HireDate,
                    IsActive = emp.isActive,
                    JobTitleId = emp.JobTitleId,
                    LeavingDate = emp.LeavingDate,
                    ManagerId = emp.ManagerId,
                    Name = emp.Name,
                    Phone = emp.Phone,
                    SectionId = emp.SectionId
                }
            };

            await PopulateEmployeeFormAsync(vm);
            return View(vm);
        }

        private async Task PopulateEmployeeFormAsync(EmployeeFormViewModel vm)
        {
            vm.JobTitles = await LoadDropdownAsync<JobTitle>(_setupService.GetJobTitleesList);
            vm.Countries = await LoadDropdownAsync<Country>(_setupService.GetCountriesList);
            vm.Sections = await LoadDropdownAsync<Section>(_setupService.GetSectionsList);
            vm.Departments = await LoadDropdownAsync<Emp.Api.Models.Department>(_setupService.GetDepartmentsList);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeFormViewModel employeeCreateDto)
        {
            if (employeeCreateDto?.Employee is null)
            {
                TempData["error"] = "No employee data submitted.";
                return RedirectToAction("Index");
            }

            var employee = new Emp.Web.Models.Dtos.EmployeeCreateDto
            {
                Id = employeeCreateDto.Employee.Id,
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

            var response = await _employeeService.EditEmployeeAsync(employee);
            if (response?.IsSuccess == true)
            {
                TempData["success"] = response.Message ?? "Employee updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = response?.Message ?? "Failed to update employee.";
            await PopulateEmployeeFormAsync(employeeCreateDto);
            return View(employeeCreateDto);
        }
        [HttpGet]
        public async Task<IActionResult> Terminate(int? employeeId)
        {
            var response = await _setupService.GetEmployeesList();
            var employeesList = JsonConvert.DeserializeObject<List<Emp.Api.Models.Employee>>(Convert.ToString(response.Result));
            ViewBag.Employees = employeesList;
            // Preselect when opened for a specific employee (e.g. from the details page).
            return View(new Emp.Web.Dtos.TerminationDto { EmployeeId = employeeId ?? 0 });
        }
        [HttpPost]
        public async Task<IActionResult> Terminate(Emp.Web.Dtos.TerminationDto terminationDto)
        {
            if (terminationDto.EmployeeId <= 0 || string.IsNullOrWhiteSpace(terminationDto.TerminationType))
            {
                ModelState.AddModelError("", "Please select an employee and a termination type.");
            }

            if (ModelState.IsValid)
            {
                var response = await _employeeService.TerminateEmployeeAsync(terminationDto.EmployeeId, terminationDto);
                if (response.IsSuccess)
                {
                    TempData["success"] = response.Message ?? "Employee terminated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", response.Message ?? "Termination failed.");
            }

            // Re-render with the employee dropdown repopulated.
            var list = await _setupService.GetEmployeesList();
            ViewBag.Employees = JsonConvert.DeserializeObject<List<Emp.Api.Models.Employee>>(Convert.ToString(list.Result));
            return View(terminationDto);
        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(int employeeId)
        {
            var response = await _employeeService.ReactivateEmployeeAsync(employeeId);
            TempData[response?.IsSuccess == true ? "success" : "error"] =
                response?.Message ?? (response?.IsSuccess == true ? "Employee reactivated." : "Reactivation failed.");
            return RedirectToAction(nameof(EmployeeDetails), new { employeeId });
        }
    }
}
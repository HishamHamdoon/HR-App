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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            Emp.Web.Models.Dtos.ResponseDto response = await _employeeService.GetEmployeesAsync(page, pageSize);
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
        public async Task<IActionResult> Create(Emp.Web.Dtos.Auth.RegisterDto employee)
        {
            var vm = new EmployeeFormViewModel
            {
                Employee = new Employee() // ensures not null
            };
            var departments = await _setupService.GetDepartmentsList();
            var countriesList = await _setupService.GetCountriesList();
            var sectionData = await _setupService.GetSectionsList();
            var jobTitlesList = await _setupService.GetJobTitleesList();
            if (jobTitlesList.Result != null)
            {
                var jobTitles = JsonConvert.DeserializeObject<List<JobTitle>>(Convert.ToString(jobTitlesList.Result));
                if (jobTitles != null)
                {
                    employee.JobTitles = jobTitles;
                }
                else
                {
                    employee.JobTitles = new List<JobTitle>();
                }
            }

            var countries = JsonConvert.DeserializeObject<List<Country>>(Convert.ToString(countriesList.Result));
            if (countries != null)
            {
                employee.Countries = countries;
            }
            else
            {
                employee.Countries = new List<Country>();
            }
            var sections = JsonConvert.DeserializeObject<List<Section>>(Convert.ToString(sectionData.Result));
            if (countries != null)
            {
                employee.Sections = sections;
            }
            else
            {
                employee.Sections = new List<Section>();
            }

            var departmentList = JsonConvert.DeserializeObject<List<Emp.Api.Models.Department>>(Convert.ToString(departments.Result));
            if (departmentList != null)
            {
                employee.Departments = departmentList;
            }
            else
            {
                employee.Departments = new List<Emp.Api.Models.Department>();
            }

            return View(employee); // shows the form
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewEmployee(Emp.Web.Dtos.Auth.RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                //Emp.Web.Models.Dtos.EmployeeCreateDto employee = new()
                //{
                //    Email = registerDto.Employee.Email,
                //    BirthDate = registerDto.Employee.BirthDate,
                //    Address = registerDto.Employee.Address,
                //    CountryId = registerDto.Employee.CountryId,
                //    DepartmentId = registerDto.Employee.DepartmentId,
                //    HireDate = registerDto.Employee.HireDate,
                //    IsActive = registerDto.Employee.IsActive,
                //    JobTitleId = registerDto.Employee.JobTitleId,
                //    LeavingDate = registerDto.Employee.LeavingDate,
                //    ManagerId = registerDto.Employee.ManagerId,
                //    Name = registerDto.Employee.Name,
                //    Phone = registerDto.Employee.Phone,


                //};
                if (registerDto.Password is null)
                {
                    registerDto.Password = "P@ssw0rd";
                }
                Emp.Web.Dtos.Auth.RegisterDto registerobj = new()
                {
                    Email = registerDto.Employee.Email,
                    BirthDate = registerDto.Employee.BirthDate,
                    Address = registerDto.Employee.Address,
                    CountryId = registerDto.Employee.CountryId,
                    DepartmentId = registerDto.Employee.DepartmentId,
                    HireDate = registerDto.Employee.HireDate,
                    IsActive = registerDto.Employee.IsActive,
                    JobTitleId = registerDto.Employee.JobTitleId,
                    LeavingDate = registerDto.Employee.LeavingDate,
                    //ManagerId = registerDto.Employee.ManagerId,
                    Name = registerDto.Employee.Name,
                    Phone = registerDto.Employee.Phone,
                    Password = registerDto.Password,
                    ManagerId = 1013,
                    PhoneNumber = "0123456789"//registerDto.Phone,
                };
                //var response = await _employeeService.CreateEmployeeAsync(employee);
                var response = await _authService.RegisterAsync(registerobj);
                TempData["Success"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                return View(registerDto);
            }
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

            var response = await _httpClient.PostAsync("https://localhost:7031/api/Employee", content);

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
            var employee = await _employeeService.GetEmployeeAsync(employeeId);

            Employee emp = JsonConvert.DeserializeObject<Employee?>(Convert.ToString(employee?.Result));
            if (emp == null)
            {
                TempData["Error"] = "Employee not found!";
            }
            var vm = new EmployeeFormViewModel
            {
                Employee = new Employee
                {
                    Id = employeeId,
                    Address = emp.Address,
                    Email = emp.Email,
                    BirthDate = emp.BirthDate,
                    Country = emp.Country,
                    CountryId = emp.CountryId,
                    Department = emp.Department,
                    DepartmentId = emp.DepartmentId,
                    HireDate = emp.HireDate,
                    IsActive = emp.IsActive,
                    JobTitle = emp.JobTitle,
                    JobTitleId = emp.JobTitleId,
                    LeavingDate = emp.LeavingDate,
                    Manager = emp.Manager,
                    ManagerId = emp.ManagerId,
                    Name = emp.Name,
                    Phone = emp.Phone,
                    Section = emp.Section,
                    SectionId = emp.SectionId,
                }
            };
            var departments = await _setupService.GetDepartmentsList();
            var countriesList = await _setupService.GetCountriesList();
            var sectionData = await _setupService.GetSectionsList();
            var jobTitlesList = await _setupService.GetJobTitleesList();
            if (jobTitlesList.Result != null)
            {
                var jobTitles = JsonConvert.DeserializeObject<List<JobTitle>>(Convert.ToString(jobTitlesList?.Result));
                if (jobTitles != null)
                {
                    vm.JobTitles = jobTitles;
                }
                else
                {
                    vm.JobTitles = new List<JobTitle>(); // Ensure it's not null
                }
            }
            var countries = JsonConvert.DeserializeObject<List<Country>>(Convert.ToString(countriesList.Result));
            if (countries != null)
            {
                vm.Countries = countries;
            }
            else
            {
                vm.Countries = new List<Country>();
            }
            var sections = JsonConvert.DeserializeObject<List<Section>>(Convert.ToString(sectionData.Result));
            if (countries != null)
            {
                vm.Sections = sections;
            }
            else
            {
                vm.Sections = new List<Section>();
            }

            var departmentList = JsonConvert.DeserializeObject<List<Emp.Api.Models.Department>>(Convert.ToString(departments.Result));
            if (departmentList != null)
            {
                vm.Departments = departmentList;
            }
            else
            {
                vm.Departments = new List<Emp.Api.Models.Department>();
            }

            return View(vm); // shows the form
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeFormViewModel employeeCreateDto)
        {
            if (ModelState.IsValid)
            {
                Emp.Web.Models.Dtos.EmployeeCreateDto employee = new()
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
                TempData["success"] = response?.Message;
                return RedirectToAction("Index");
            }

            else
            {
                return View(employeeCreateDto);
            }
            //return View(employee);
        }
        [HttpPost]
        public async Task<IActionResult> SetManager(int employeeId, int managerId)
        {
            if (employeeId > 0 && managerId > 0)
            {
                var response = await _employeeService.SetManager(employeeId, managerId);
                if (response.IsSuccess)
                {
                    TempData["Success"] = "Manager set ";//response.Message;
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "Something went wrong";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Terminate()
        {
            var response = await _setupService.GetEmployeesList();
            var employeesList = JsonConvert.DeserializeObject<List<Emp.Api.Models.Employee>>(Convert.ToString(response.Result));
            ViewBag.Employees = employeesList;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Terminate(Emp.Web.Dtos.TerminationDto terminationDto)
        {

            var response = await _employeeService.TerminateEmployeeAsync(terminationDto.EmployeeId, terminationDto);
            if (response.IsSuccess)
            {
                TempData["Success"] = response.Message;
                return RedirectToAction(nameof(Index));
                //return Json(new { success = true, message = response.Message });
            }
            else
            {
                return View(terminationDto);
                //return Json(new { success = false, message = response.Message });
            }
        }
    }
}
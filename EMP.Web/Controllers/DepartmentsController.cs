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
            ResponseDto response = await _departmentService.GetDepartmentAsync(id);
            if (response is not null && response.IsSuccess)
            {
                var department= JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentDto>(Convert.ToString(response.Result));
                return View(department);
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> DeleteEmployeeGet(int employeeId)
        {
            if (employeeId > 0)
            {
                ResponseDto response = await _employeeService.GetEmployeeAsync(employeeId);
                if (response is not null && response.IsSuccess)
                {
                    EmployeeVM employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response.Result));
                    return View(employee);
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int Id)
        {
            //ResponseDto response = await _employeeService.GetEmployeeAsync(Id);
            //Console.WriteLine(response.Result);
            if (Id>0)
            {
                var deleteResponse = await _employeeService.DeleteEmployeeAsync(Id);
                if (deleteResponse.IsSuccess)
                {
                    TempData["success"] = deleteResponse.Message;
                    return RedirectToAction(nameof(Index));
                }
                TempData["error"] = deleteResponse.Message;
                return View(nameof(Details));
            }
            return View("Index");
        }

        //[HttpGet]
        //public async Task<IActionResult> Create(EmployeeFormViewModel employee)
        //{
        //    var vm = new EmployeeFormViewModel
        //    {
        //        Employee = new Employee() // ensures not null
        //    };
        //    var departments = await _departmentService.GetDepartmentsAsync();
        //    var countriesList = await _countryService.GetCountriesAsync();
        //    var sectionData = await _sectionService.GetSectionsAsync();
        //    var jobTitlesList = await _jobTitleService.GetJobTitlesAsync();

        //    // Fix for CS0029, CS8601, and CS8604
        //    if (jobTitlesList.Result != null)
        //    {
        //        var jobTitles = JsonConvert.DeserializeObject<List<JobTitle>>(Convert.ToString(jobTitlesList.Result));
        //        if (jobTitles != null)
        //        {
        //            //SelectListItem selectListItem = new SelectListItem();
        //            //selectListItem.Text = jobTitles[0].Title;
        //            //selectListItem.Value = jobTitles[0].Id.ToString();
        //            employee.JobTitles = jobTitles;
        //        }
        //        else
        //        {
        //            employee.JobTitles = new List<JobTitle>(); // Ensure it's not null
        //        }
        //    }

        //    var countries = JsonConvert.DeserializeObject<List<Country>>(Convert.ToString(countriesList.Result));
        //    if (countries != null)
        //    {
        //        employee.Countries = countries;
        //    }
        //    else
        //    {
        //        employee.Countries = new List<Country>();
        //    }
        //    var sections = JsonConvert.DeserializeObject<List<Section>>(Convert.ToString(sectionData.Result));
        //    if (countries != null)
        //    {
        //        employee.Sections = sections;
        //    }
        //    else
        //    {
        //        employee.Sections = new List<Section>();
        //    }

        //    var departmentList = JsonConvert.DeserializeObject<List<Emp.Api.Models.Department>>(Convert.ToString(departments.Result));
        //    if (departmentList != null)
        //    {
        //        employee.Departments = departmentList;
        //    }
        //    else
        //    {
        //        employee.Departments = new List<Emp.Api.Models.Department>();
        //    }

        //    return View(employee); // shows the form
        //}
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
        [HttpPost]
        public async Task<IActionResult> CreatePost(EmployeeFormViewModel model)
        {
            // Optional: validate
            //if (!ModelState.IsValid)
            //{
            //    model.Departments = await GetDepartmentsAsync();
            //    model.Sections = await GetSectionsAsync();
            //    model.JobTitles = await GetJobTitlesAsync();
            //    return View("Create", model);
            //}

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
            var emp = JsonConvert.DeserializeObject<Employee>(Convert.ToString(employee.Result));
            var vm = new EmployeeFormViewModel
            {
                Employee = new Employee { Id = employeeId,
                Address=emp.Address,
                Email=emp.Email,
                BirthDate=emp.BirthDate,
                Country=emp.Country,
                CountryId=emp.CountryId,
                Department = emp.Department,
                DepartmentId=emp.DepartmentId,
                HireDate=emp.HireDate,
                IsActive=emp.IsActive,
                JobTitle=emp.JobTitle,
                JobTitleId=emp.JobTitleId,
                LeavingDate=emp.LeavingDate,
                Manager = emp.Manager,
                ManagerId=emp.ManagerId,
                Name=emp.Name,
                Phone=emp.Phone,
                Section=emp.Section,
                SectionId=emp.SectionId,              
                }
            };           
            var departments = await _departmentService.GetDepartmentsAsync();
            var countriesList = await _countryService.GetCountriesAsync();
            var sectionData = await _sectionService.GetSectionsAsync();
            var jobTitlesList = await _jobTitleService.GetJobTitlesAsync();
            if (jobTitlesList.Result != null)
            {
                var jobTitles = JsonConvert.DeserializeObject<List<JobTitle>>(Convert.ToString(jobTitlesList?.Result));
                if (jobTitles != null)
                {
                    //SelectListItem selectListItem = new SelectListItem();
                    //selectListItem.Text = jobTitles[0].Title;
                    //selectListItem.Value = jobTitles[0].Id.ToString();
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
                    Id=employeeCreateDto.Employee.Id,
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
                TempData["Success"] = response?.Message;
                return RedirectToAction("Index");
            }
        
            else
            {
                return View(employeeCreateDto);
            }
            //return View(employee);
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

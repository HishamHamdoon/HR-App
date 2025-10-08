using Emp.Models;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EMP.Web.Controllers
{
    public class SalariesController : Controller
    {
        private readonly ISalaryService _salaryService;
        private readonly ISetupService _setupService;

        public SalariesController(ISalaryService salaryService,ISetupService setupService) 
        {
            _salaryService = salaryService;
            _setupService = setupService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _salaryService.GetSalariesAsync();
            if (response.IsSuccess && response.Result != null)
            {
                var salaries = JsonConvert.DeserializeObject<List<SalaryDto>>(response.Result?.ToString());
                return View(salaries);
            }
            return View(null);
        }
        public async Task<IActionResult> Details(int salaryId)
        {
            var response = await _salaryService.GetSalaryAsync(salaryId);
            if (response.IsSuccess && response.Result != null)
            {
                var salary = JsonConvert.DeserializeObject<SalaryDto>(response.Result?.ToString());
                return View(salary);
            }
            return View(null);
        }

        public async Task<IActionResult> Create()
        {
            var employeeResponse = await _setupService.GetEmployeesList();

            if (employeeResponse.IsSuccess && employeeResponse.Result != null)
            {
                // Deserialize JSON array to List<SalaryEmployeeDto> or anonymous object
                var employees = JsonConvert.DeserializeObject<List<EmployeeDto>>(employeeResponse.Result.ToString());

                // Convert to SelectList for dropdown
                ViewBag.Employees = new SelectList(employees, "Id", "Name");
            }
            else
            {
                ViewBag.Employees = new SelectList(new List<EmployeeDto>(), "Id", "Name");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSalaryDto salaryDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _salaryService.CreateSalaryAsync(salaryDto);

                if (response.IsSuccess && response.Result != null)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = response.Message;
                    return View(salaryDto);
                }
            }
            else
            {
                var employeeResponse = await _setupService.GetEmployeesList();

                if (employeeResponse.IsSuccess && employeeResponse.Result != null)
                {
                    // Deserialize JSON array to List<SalaryEmployeeDto> or anonymous object
                    var employees = JsonConvert.DeserializeObject<List<EmployeeDto>>(employeeResponse.Result.ToString());

                    // Convert to SelectList for dropdown
                    ViewBag.Employees = new SelectList(employees, "Id", "Name");
                }
                else
                {
                    ViewBag.Employees = new SelectList(new List<EmployeeDto>(), "Id", "Name");
                }
                return View(salaryDto);
            }
                
        }

        public async Task<IActionResult> Edit(int salaryId)
        {
            var response = await _salaryService.GetSalaryAsync(salaryId);
            if (response.IsSuccess && response.Result != null)
            {
                var salary = JsonConvert.DeserializeObject<SalaryDto>(response.Result?.ToString());
                return View(salary);
            }
            return View(null);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(SalaryDto salaryDto)
        {
            //
            if (ModelState.IsValid)
            {
                var response = await _salaryService.UpdateSalaryAsync(salaryDto);
                if (response.IsSuccess && response.Result != null)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    var salary = JsonConvert.DeserializeObject<SalaryDto>(response.Result?.ToString());
                    TempData["Error"] = response.Message;
                    return View(salary);
                }
            }
           
            return View(null);
        }
        //[HttpPost]
        public async Task<IActionResult> Delete(int salaryId)
        {
            if (salaryId > 0)
            {
                var response = await _salaryService.DeleteSalaryAsync(salaryId);
                if (response.IsSuccess && response.Result != null)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(null);
        }
    }
}

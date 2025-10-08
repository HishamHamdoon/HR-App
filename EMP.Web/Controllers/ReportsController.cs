using Emp.Api.Dtos.Department;
using Emp.Api.Dtos.Employee;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using EMP.Web.Services.Reports;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly EmployeeReportService _reportService;
        private readonly DepartmentReportService _departmentReportService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public ReportsController(EmployeeReportService reportService,
            IDepartmentService departmentService,
            IHttpClientFactory httpClientFactory,IEmployeeService employeeService, DepartmentReportService departmentReportService)
        {
            _reportService = reportService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _departmentReportService = departmentReportService;
            //_httpClient = httpClientFactory.CreateClient("HRApi"); // register API client in Program.cs
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> EmployeeReport()
        {
            // Call your service
            var response = await _employeeService.GetEmployeesAsync(1, 20);

            var employees = new List<EmployeeViewDto>();

            if (response != null && response.IsSuccess && response.Result != null)
            {
                // Deserialize Result into PagedApiResponse<EmployeeViewDto>
                var pagedResult = JsonConvert.DeserializeObject<PagedApiResponse<EmployeeViewDto>>(response.Result.ToString());

                if (pagedResult?.Data != null)
                {
                    employees = pagedResult.Data;  // ✅ now you have the actual list
                }
            }

            var pdfBytes = _reportService.GenerateEmployeeReport(employees);

            return File(pdfBytes, "application/pdf", "EmployeeReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DepartmentReport()
        {
            // Call your service
            var response = await _departmentService.GetDepartmentsAsync();

            var departments = new List<Emp.Web.Dtos.DepartmentDto>();

            if (response != null && response.IsSuccess && response.Result != null)
            {
                // Deserialize Result into PagedApiResponse<EmployeeViewDto>
                var result = JsonConvert.DeserializeObject<List<Emp.Web.Dtos.DepartmentDto>> (response?.Result?.ToString());

                if (result != null)
                {
                    departments = result;  // ✅ now you have the actual list
                }
            }

            var pdfBytes = _departmentReportService.GenerateDepartmentReport(departments);

            return File(pdfBytes, "application/pdf", "DepartmentReport.pdf");
        }

    }
}

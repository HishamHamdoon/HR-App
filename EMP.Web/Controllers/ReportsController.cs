using ClosedXML.Excel;
using Emp.Api.Dtos.Employee;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using EMP.Web.Services.Reports;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EMP.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly EmployeeReportService _reportService;
        private readonly DepartmentReportService _departmentReportService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ILeaveService _leaveService;

        public ReportsController(EmployeeReportService reportService,
            IDepartmentService departmentService,
            IHttpClientFactory httpClientFactory,
            IEmployeeService employeeService,
            DepartmentReportService departmentReportService,
            ILeaveService leaveService)
        {
            _reportService = reportService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _departmentReportService = departmentReportService;
            _leaveService = leaveService;
        }

        public IActionResult Index() => View();

        // ---------- data helpers ----------

        private async Task<List<EmployeeViewDto>> GetEmployeesAsync()
        {
            var response = await _employeeService.GetEmployeesAsync(1, 1000);
            if (response?.IsSuccess == true && response.Result != null)
            {
                var paged = JsonConvert.DeserializeObject<PagedApiResponse<EmployeeViewDto>>(response.Result.ToString());
                return paged?.Data ?? new List<EmployeeViewDto>();
            }
            return new List<EmployeeViewDto>();
        }

        private async Task<List<Emp.Web.Dtos.DepartmentDto>> GetDepartmentsAsync()
        {
            var response = await _departmentService.GetDepartmentsAsync();
            if (response?.IsSuccess == true && response.Result != null)
            {
                return JsonConvert.DeserializeObject<List<Emp.Web.Dtos.DepartmentDto>>(response.Result.ToString())
                       ?? new List<Emp.Web.Dtos.DepartmentDto>();
            }
            return new List<Emp.Web.Dtos.DepartmentDto>();
        }

        private async Task<List<Emp.Web.Dtos.ViewLeaveDto>> GetLeavesAsync()
        {
            var response = await _leaveService.GetLeavesAsync(1, 1000);
            if (response?.IsSuccess == true && response.Result != null)
            {
                var paged = JsonConvert.DeserializeObject<PagedApiResponse<Emp.Web.Dtos.ViewLeaveDto>>(response.Result.ToString());
                return paged?.Data ?? new List<Emp.Web.Dtos.ViewLeaveDto>();
            }
            return new List<Emp.Web.Dtos.ViewLeaveDto>();
        }

        // ---------- generic Excel builder ----------

        private static byte[] BuildExcel(string sheetName, string[] headers, IEnumerable<string[]> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(sheetName);

            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cell(1, c + 1).Value = headers[c];
            }
            var headerRange = ws.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var r in rows)
            {
                for (int c = 0; c < r.Length; c++)
                {
                    ws.Cell(row, c + 1).Value = r[c];
                }
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ---------- Employee ----------

        [HttpGet]
        public async Task<IActionResult> EmployeeReport()
        {
            var employees = await GetEmployeesAsync();
            var pdf = _reportService.GenerateEmployeeReport(employees);
            return File(pdf, "application/pdf", "EmployeeReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeExcelReport()
        {
            var employees = await GetEmployeesAsync();
            var bytes = BuildExcel("Employees",
                new[] { "#", "Name", "Email", "Department", "Job Title", "Country", "Status" },
                employees.Select((e, i) => new[]
                {
                    (i + 1).ToString(), e.Name ?? "", e.Email ?? "", e.DepartmentName ?? "",
                    e.JobTitleTitle ?? "", e.CountryName ?? "", e.isActive ? "Active" : "Inactive"
                }));
            return File(bytes, XlsxContentType, "EmployeeReport.xlsx");
        }

        // ---------- Department ----------

        [HttpGet]
        public async Task<IActionResult> DepartmentReport()
        {
            var departments = await GetDepartmentsAsync();
            var pdf = _departmentReportService.GenerateDepartmentReport(departments);
            return File(pdf, "application/pdf", "DepartmentReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DepartmentExcelReport()
        {
            var departments = await GetDepartmentsAsync();
            var bytes = BuildExcel("Departments",
                new[] { "#", "Name" },
                departments.Select((d, i) => new[] { (i + 1).ToString(), d.Name ?? "" }));
            return File(bytes, XlsxContentType, "DepartmentReport.xlsx");
        }

        // ---------- Leave ----------

        [HttpGet]
        public async Task<IActionResult> LeaveReport()
        {
            var leaves = await GetLeavesAsync();
            var pdf = GenerateLeavePdf(leaves);
            return File(pdf, "application/pdf", "LeaveReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> LeaveExcelReport()
        {
            var leaves = await GetLeavesAsync();
            var bytes = BuildExcel("Leaves",
                new[] { "#", "Employee", "Leave Type", "Start", "End", "Status", "Manager" },
                leaves.Select((l, i) => new[]
                {
                    (i + 1).ToString(), l.EmployeeName ?? "", l.LeaveName ?? "",
                    l.StartDate.ToString("yyyy-MM-dd"), l.EndDate?.ToString("yyyy-MM-dd") ?? "",
                    l.Status ?? "Pending", l.ManagerName ?? ""
                }));
            return File(bytes, XlsxContentType, "LeaveReport.xlsx");
        }

        private static byte[] GenerateLeavePdf(List<Emp.Web.Dtos.ViewLeaveDto> leaves)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Leave Report").FontSize(20).Bold().AlignCenter();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        table.Header(header =>
                        {
                            foreach (var h in new[] { "#", "Employee", "Leave Type", "Start", "End", "Status" })
                            {
                                header.Cell().Element(c => c.PaddingVertical(5).PaddingHorizontal(3)
                                    .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .DefaultTextStyle(x => x.SemiBold())).Text(h);
                            }
                        });
                        int index = 1;
                        foreach (var l in leaves)
                        {
                            void Cell(string text) => table.Cell().Element(c => c.PaddingVertical(4).PaddingHorizontal(3)
                                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)).Text(text);
                            Cell((index++).ToString());
                            Cell(l.EmployeeName ?? "");
                            Cell(l.LeaveName ?? "");
                            Cell(l.StartDate.ToString("yyyy-MM-dd"));
                            Cell(l.EndDate?.ToString("yyyy-MM-dd") ?? "");
                            Cell(l.Status ?? "Pending");
                        }
                    });
                    page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            });
            return document.GeneratePdf();
        }
    }
}

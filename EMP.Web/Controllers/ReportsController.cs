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

        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ILeaveService _leaveService;
        private readonly ISettingsService _settingsService;

        public ReportsController(
            IDepartmentService departmentService,
            IEmployeeService employeeService,
            ILeaveService leaveService,
            ISettingsService settingsService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _leaveService = leaveService;
            _settingsService = settingsService;
        }

        // Branded report header context: company name + logo from settings, who generated it,
        // and the reporting period (when supplied).
        private async Task<ReportContext> BuildContextAsync(string title, DateTime? from = null, DateTime? to = null)
        {
            var settings = await _settingsService.GetSettingsAsync();
            byte[]? logo = null;
            if (!string.IsNullOrWhiteSpace(settings.LogoBase64))
            {
                try { logo = Convert.FromBase64String(settings.LogoBase64); } catch { logo = null; }
            }
            return new ReportContext
            {
                CompanyName = string.IsNullOrWhiteSpace(settings.CompanyName) ? "Your Company" : settings.CompanyName,
                Logo = logo,
                Title = title,
                From = from,
                To = to,
                CreatedBy = User.Identity?.Name ?? "—",
            };
        }

        private static bool InRange(DateTime date, DateTime? from, DateTime? to) =>
            (!from.HasValue || date.Date >= from.Value.Date) && (!to.HasValue || date.Date <= to.Value.Date);

        private static bool IsPending(string? status) =>
            string.IsNullOrEmpty(status) || status.Equals("Pending", StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> Index()
        {
            var employees = await GetEmployeesAsync();
            var leaves = await GetLeavesAsync();

            // Headcount per department.
            ViewBag.HeadcountByDept = employees
                .GroupBy(e => string.IsNullOrEmpty(e.DepartmentName) ? "Unassigned" : e.DepartmentName)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .ToList();

            ViewBag.ActiveCount = employees.Count(e => e.isActive);
            ViewBag.InactiveCount = employees.Count(e => !e.isActive);

            // Leave status breakdown (case-insensitive).
            string Norm(string? s) => string.IsNullOrEmpty(s) ? "Pending"
                : char.ToUpper(s[0]) + s.Substring(1).ToLower();
            ViewBag.LeaveByStatus = leaves
                .GroupBy(l => Norm(l.Status))
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .ToList();

            // Leave utilisation by type.
            ViewBag.LeaveByType = leaves
                .GroupBy(l => string.IsNullOrEmpty(l.LeaveName) ? "—" : l.LeaveName)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .ToList();

            return View();
        }

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

        private async Task<List<TerminationRowDto>> GetTerminationsAsync()
        {
            var response = await _employeeService.GetTerminationsAsync();
            if (response?.IsSuccess == true && response.Result != null)
            {
                return JsonConvert.DeserializeObject<List<TerminationRowDto>>(response.Result.ToString())
                       ?? new List<TerminationRowDto>();
            }
            return new List<TerminationRowDto>();
        }

        // ---------- generic Excel builder ----------

        private static byte[] BuildExcel(ReportContext ctx, string sheetName, string[] headers, IEnumerable<string[]> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(sheetName);

            // Branded meta rows.
            ws.Cell(1, 1).Value = ctx.CompanyName;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = ctx.Title;
            ws.Cell(2, 1).Style.Font.Bold = true;
            var period = (ctx.From.HasValue || ctx.To.HasValue)
                ? $"Period: {(ctx.From.HasValue ? ctx.From.Value.ToString("yyyy-MM-dd") : "…")} to {(ctx.To.HasValue ? ctx.To.Value.ToString("yyyy-MM-dd") : "…")}"
                : "Period: All";
            ws.Cell(3, 1).Value = $"{period}   |   Generated by {ctx.CreatedBy} on {DateTime.Now:yyyy-MM-dd HH:mm}";

            const int headerRow = 5;
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cell(headerRow, c + 1).Value = headers[c];
            }
            var headerRange = ws.Range(headerRow, 1, headerRow, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = headerRow + 1;
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
            var ctx = await BuildContextAsync("Employee Report");
            var pdf = PdfReportBuilder.BuildTable(ctx,
                new[] { "#", "Name", "Email", "Department", "Job Title", "Country", "Status" },
                employees.Select((e, i) => new[]
                {
                    (i + 1).ToString(), e.Name ?? "", e.Email ?? "", e.DepartmentName ?? "",
                    e.JobTitleTitle ?? "", e.CountryName ?? "", e.isActive ? "Active" : "Inactive"
                }));
            return File(pdf, "application/pdf", "EmployeeReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeExcelReport()
        {
            var employees = await GetEmployeesAsync();
            var ctx = await BuildContextAsync("Employee Report");
            var bytes = BuildExcel(ctx, "Employees",
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
            var ctx = await BuildContextAsync("Department Report");
            var pdf = PdfReportBuilder.BuildTable(ctx,
                new[] { "#", "Name" },
                departments.Select((d, i) => new[] { (i + 1).ToString(), d.Name ?? "" }));
            return File(pdf, "application/pdf", "DepartmentReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DepartmentExcelReport()
        {
            var departments = await GetDepartmentsAsync();
            var ctx = await BuildContextAsync("Department Report");
            var bytes = BuildExcel(ctx, "Departments",
                new[] { "#", "Name" },
                departments.Select((d, i) => new[] { (i + 1).ToString(), d.Name ?? "" }));
            return File(bytes, XlsxContentType, "DepartmentReport.xlsx");
        }

        // ---------- Leave ----------

        [HttpGet]
        public async Task<IActionResult> LeaveReport(DateTime? from, DateTime? to)
        {
            var leaves = (await GetLeavesAsync()).Where(l => InRange(l.StartDate, from, to)).ToList();
            var ctx = await BuildContextAsync("Leave Report", from, to);
            var pdf = PdfReportBuilder.BuildTable(ctx,
                new[] { "#", "Employee", "Leave Type", "Start", "End", "Status" },
                leaves.Select((l, i) => new[]
                {
                    (i + 1).ToString(), l.EmployeeName ?? "", l.LeaveName ?? "",
                    l.StartDate.ToString("yyyy-MM-dd"), l.EndDate?.ToString("yyyy-MM-dd") ?? "", l.Status ?? "Pending"
                }));
            return File(pdf, "application/pdf", "LeaveReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> LeaveExcelReport(DateTime? from, DateTime? to)
        {
            var leaves = (await GetLeavesAsync()).Where(l => InRange(l.StartDate, from, to)).ToList();
            var ctx = await BuildContextAsync("Leave Report", from, to);
            var bytes = BuildExcel(ctx, "Leaves",
                new[] { "#", "Employee", "Leave Type", "Start", "End", "Status", "Manager" },
                leaves.Select((l, i) => new[]
                {
                    (i + 1).ToString(), l.EmployeeName ?? "", l.LeaveName ?? "",
                    l.StartDate.ToString("yyyy-MM-dd"), l.EndDate?.ToString("yyyy-MM-dd") ?? "",
                    l.Status ?? "Pending", l.ManagerName ?? ""
                }));
            return File(bytes, XlsxContentType, "LeaveReport.xlsx");
        }

        // ---------- Terminated Employees ----------

        [HttpGet]
        public async Task<IActionResult> TerminatedReport()
        {
            var rows = await GetTerminationsAsync();
            var ctx = await BuildContextAsync("Terminated Employees Report");
            var pdf = PdfReportBuilder.BuildTable(ctx,
                new[] { "#", "Name", "Department", "Type", "Reason", "Date" },
                rows.Select((t, i) => new[]
                {
                    (i + 1).ToString(), t.Name, t.Department, t.TerminationType, t.TerminationReason, t.DateTerminated
                }));
            return File(pdf, "application/pdf", "TerminatedEmployeesReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> TerminatedExcelReport()
        {
            var rows = await GetTerminationsAsync();
            var ctx = await BuildContextAsync("Terminated Employees Report");
            var bytes = BuildExcel(ctx, "Terminated",
                new[] { "#", "Name", "Department", "Type", "Reason", "Date" },
                rows.Select((t, i) => new[]
                {
                    (i + 1).ToString(), t.Name, t.Department, t.TerminationType, t.TerminationReason, t.DateTerminated
                }));
            return File(bytes, XlsxContentType, "TerminatedEmployeesReport.xlsx");
        }

        // ---------- Pending Leaves ----------

        [HttpGet]
        public async Task<IActionResult> PendingLeaveReport(DateTime? from, DateTime? to)
        {
            var leaves = (await GetLeavesAsync())
                .Where(l => IsPending(l.Status) && InRange(l.StartDate, from, to)).ToList();
            var ctx = await BuildContextAsync("Pending Leave Requests", from, to);
            var pdf = PdfReportBuilder.BuildTable(ctx,
                new[] { "#", "Employee", "Leave Type", "Start", "End", "Manager" },
                leaves.Select((l, i) => new[]
                {
                    (i + 1).ToString(), l.EmployeeName ?? "", l.LeaveName ?? "",
                    l.StartDate.ToString("yyyy-MM-dd"), l.EndDate?.ToString("yyyy-MM-dd") ?? "", l.ManagerName ?? ""
                }));
            return File(pdf, "application/pdf", "PendingLeaveReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> PendingLeaveExcelReport(DateTime? from, DateTime? to)
        {
            var leaves = (await GetLeavesAsync())
                .Where(l => IsPending(l.Status) && InRange(l.StartDate, from, to)).ToList();
            var ctx = await BuildContextAsync("Pending Leave Requests", from, to);
            var bytes = BuildExcel(ctx, "Pending Leaves",
                new[] { "#", "Employee", "Leave Type", "Start", "End", "Manager" },
                leaves.Select((l, i) => new[]
                {
                    (i + 1).ToString(), l.EmployeeName ?? "", l.LeaveName ?? "",
                    l.StartDate.ToString("yyyy-MM-dd"), l.EndDate?.ToString("yyyy-MM-dd") ?? "", l.ManagerName ?? ""
                }));
            return File(bytes, XlsxContentType, "PendingLeaveReport.xlsx");
        }
    }
}

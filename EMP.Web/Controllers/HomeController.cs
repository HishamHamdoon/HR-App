using Emp.Api.Controllers;
using Emp.Web.Dtos;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveService _leaveService;
        public HomeController(ILogger<HomeController>  logger,ILeaveService leaveService,IEmployeeService employeeService, HttpClient httpClient, IStringLocalizer<HomeController> localizer)
        {
            _httpClient = httpClient;
            _employeeService = employeeService;
            _leaveService = leaveService;
            _logger = logger;
            _localizer = localizer;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Non-admins get their own dashboard, not the admin overview.
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(EmployeeDashboard));
            }

            // Sensible defaults so empty DB / failed API call still render the cards.
            ViewBag.ActiveLeaves = 0;
            ViewBag.Employees = 0;
            ViewBag.Departments = 0;
            ViewBag.Pending = 0;
            ViewBag.ChartsJson = "null";

            try
            {
                var response = await _employeeService.GetDashboardAsync();
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    var dashboardCounts = JsonConvert.DeserializeObject<DashboardCountsDto>(response.Result.ToString());
                    if (dashboardCounts is not null)
                    {
                        ViewBag.ActiveLeaves = dashboardCounts.ActiveLeaves;
                        ViewBag.Employees = dashboardCounts.TotalEmployees;
                        ViewBag.Departments = dashboardCounts.Departments;
                        ViewBag.Pending = dashboardCounts.PendingApprovals;
                    }
                }

                var charts = await _employeeService.GetDashboardChartsAsync();
                if (charts?.IsSuccess == true && charts.Result is not null)
                {
                    // Pass the raw chart payload straight to the view as JSON for Chart.js.
                    ViewBag.ChartsJson = JsonConvert.SerializeObject(charts.Result);
                }
            }
            catch
            {
                // Defaults above stay; dashboard still renders.
            }

            return View("Dashboard");
        }
       
        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Createemployee()
        {
            return View(); // shows the form
        }
        
        public IActionResult ChangeLang(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                lang = "en";
            }

            var culture = new RequestCulture(lang);
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(culture),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            // Redirect to previous page
            var referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
        }

        [Authorize]
        public async Task<IActionResult> EmployeeDashboard()
        {
            var leavesList = new List<ViewLeaveDto>();
            ViewBag.Balances = new List<EMP.Web.Models.Dtos.LeaveBalanceDto>();
            ViewBag.PendingApprovals = new List<EMP.Web.Models.Dtos.LeaveByManagerDto>();

            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(employeeIdClaim, out var employeeId) && employeeId > 0)
            {
                try
                {
                    var response = await _leaveService.GetLeavesByEmployeeIdAsync(employeeId);
                    if (response?.IsSuccess == true && response.Result is not null)
                    {
                        leavesList = JsonConvert.DeserializeObject<List<ViewLeaveDto>>(
                                         JsonConvert.SerializeObject(response.Result))
                                     ?? new List<ViewLeaveDto>();
                    }

                    var balanceResp = await _leaveService.GetLeaveBalanceAsync(employeeId);
                    if (balanceResp?.IsSuccess == true && balanceResp.Result is not null)
                    {
                        ViewBag.Balances = JsonConvert.DeserializeObject<List<EMP.Web.Models.Dtos.LeaveBalanceDto>>(
                                               JsonConvert.SerializeObject(balanceResp.Result))
                                           ?? new List<EMP.Web.Models.Dtos.LeaveBalanceDto>();
                    }

                    // If this employee manages others, surface the leave requests awaiting their approval.
                    var managerResp = await _leaveService.GetLeavesByManagerAsync(employeeId);
                    if (managerResp?.IsSuccess == true && managerResp.Result is not null)
                    {
                        var managed = JsonConvert.DeserializeObject<List<EMP.Web.Models.Dtos.LeaveByManagerDto>>(
                                          JsonConvert.SerializeObject(managerResp.Result))
                                      ?? new List<EMP.Web.Models.Dtos.LeaveByManagerDto>();
                        ViewBag.PendingApprovals = managed
                            .Where(l => string.IsNullOrEmpty(l.Status)
                                        || l.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                                        || l.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }
                }
                catch
                {
                    // Fall through with empty data — the view renders zero counts.
                }
            }

            return View(leavesList);
        }
    }
}

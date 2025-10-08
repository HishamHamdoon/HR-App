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
        //[Authorize(Roles = "Admin")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var response = await _employeeService.GetDashboardAsync();
            if (response.IsSuccess)
            {
                var dashboardCounts = JsonConvert.DeserializeObject<DashboardCountsDto>(response.Result.ToString());
                ViewBag.ActiveLeaves = dashboardCounts.ActiveLeaves;
                ViewBag.Employees = dashboardCounts.TotalEmployees;
                ViewBag.Departments = dashboardCounts.Departments;
                ViewBag.Pending = dashboardCounts.PendingApprovals;
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
            var EmployeeId = User.FindFirst("EmployeeId")?.Value;
            var response = await _leaveService.GetLeavesByEmployeeIdAsync(employeeId:int.Parse(EmployeeId));
            if (response.IsSuccess)
            {
                var leavesList = JsonConvert.DeserializeObject<List<ViewLeaveDto>>(
                                JsonConvert.SerializeObject(response.Result));
                return View(leavesList);
            }
            return View(null);
        }
    }
}

using Emp.Api.Controllers;
using EMP.Web.Services.IServices;
using EMP.Web.Views.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    // Department managers (Employee role with the IsManager flag) can view the employees
    // who report to them. Read-only — all employee CRUD stays with Admins.
    [Authorize]
    public class MyTeamController : BaseController
    {
        private readonly IEmployeeService _employeeService;

        public MyTeamController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.HasClaim("IsManager", "true"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var team = new List<EmployeeVM>();

            if (int.TryParse(User.FindFirst("EmployeeId")?.Value, out var managerId) && managerId > 0)
            {
                var response = await _employeeService.GetByManagerAsync(managerId);
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    team = JsonConvert.DeserializeObject<List<EmployeeVM>>(Convert.ToString(response.Result))
                           ?? new List<EmployeeVM>();
                }
            }

            return View(team);
        }
    }
}

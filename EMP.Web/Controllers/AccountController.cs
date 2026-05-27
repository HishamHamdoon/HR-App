using Azure;
using Emp.Api.Controllers;
using Emp.Web.Dtos.Auth;
using EMP.Web.Models;
using EMP.Web.Services.IServices;
using EMP.Web.Views.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMP.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
          public AccountController(HttpClient httpClient,IAccountService accountService,ITokenProvider tokenProvider,IEmployeeService employeeService,IDepartmentService departmentService)
        {
            _httpClient = httpClient;
            _accountService = accountService;
            _tokenProvider =tokenProvider;
            _employeeService = employeeService;
            _departmentService = departmentService;

        }
        public IActionResult Index()
        {
            return View();
        }
        private readonly HttpClient _httpClient;

      

        [HttpGet]
        public IActionResult Login() => View();

      
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _accountService.LoginAsync(model);

            if (response is null)
            {
                ViewBag.Errors = new List<string> { "No response from authentication service." };
                return View(model);
            }

            if (!response.IsSuccess || response.Result is null)
            {
                var apiMessage = string.IsNullOrWhiteSpace(response.Message) ? "Invalid login attempt." : response.Message;
                ViewBag.Errors = new List<string> { apiMessage };
                return View(model);
            }

            Emp.Web.Dtos.Auth.LoginResponseDto? loginResponse;
            try
            {
                loginResponse = JsonConvert.DeserializeObject<Emp.Web.Dtos.Auth.LoginResponseDto>(response.Result.ToString());
            }
            catch (Exception ex)
            {
                ViewBag.Errors = new List<string> { $"Failed to parse login response: {ex.Message}" };
                return View(model);
            }

            if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.Token))
            {
                ViewBag.Errors = new List<string> { "Login succeeded but no token was returned." };
                return View(model);
            }

            await SignIn(loginResponse);
            _tokenProvider.SetToken(loginResponse.Token);
            var roles = GetRoleFromToken(loginResponse.Token);
            if (roles.Contains("Employee"))
            {
                return RedirectToAction("EmployeeDashboard", "Home");
            }
            return RedirectToAction("Index", "Home");
        }



        //public async Task<IActionResult> Login(LoginRequestDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);
        //    var content = new StringContent(
        //        JsonSerializer.Serialize(model),
        //        Encoding.UTF8,
        //        "application/json"
        //    );

        //    var response = await _httpClient.PostAsync("https://localhost:7031/api/Auth/login", content);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorText = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine("Login failed response: " + errorText);
        //        ModelState.AddModelError("", "Login failed: " + errorText);
        //        return View(model);
        //    }

        //    // Get the plain string token from the response
        //    var token = await response.Content.ReadAsStringAsync();

        //    // Optional: remove surrounding quotes if it's returned as a JSON string literal
        //    token = token.Trim('"');

        //    // Save token to cookie
        //    Response.Cookies.Append("jwt", token, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true, // recommended for HTTPS
        //        SameSite = SameSiteMode.Strict
        //    });

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{Emp.Web.Utility.SD.ApiBaseUrl}/api/Auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Registration failed.");
                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return RedirectToAction("Login", "Account");
        }

        private async Task SignIn(Emp.Web.Dtos.Auth.LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(model.Token);

            var claims = new List<Claim>();

            // Standard claims
            var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

            if (!string.IsNullOrEmpty(email)) claims.Add(new Claim(ClaimTypes.Email, email));
            if (!string.IsNullOrEmpty(sub)) claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
            if (!string.IsNullOrEmpty(name)) claims.Add(new Claim(ClaimTypes.Name, name));
            // 👇 Add EmployeeId claim
            var empId = jwt.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(empId))
            {
                claims.Add(new Claim("EmployeeId", empId));
            }
            // 👇 Manager flag (drives manager-only UI such as Team Leaves)
            var isManager = jwt.Claims.FirstOrDefault(c => c.Type == "IsManager")?.Value;
            if (!string.IsNullOrEmpty(isManager))
            {
                claims.Add(new Claim("IsManager", isManager));
            }
            // 👇 First-login password-change flag
            var mustChange = jwt.Claims.FirstOrDefault(c => c.Type == "MustChangePassword")?.Value;
            if (!string.IsNullOrEmpty(mustChange))
            {
                claims.Add(new Claim("MustChangePassword", mustChange));
            }
            // 👇 Apply the user's UI preferences (theme + calendar) via cookies the layout reads.
            var prefTheme = jwt.Claims.FirstOrDefault(c => c.Type == "PreferredTheme")?.Value;
            var prefCalendar = jwt.Claims.FirstOrDefault(c => c.Type == "Calendar")?.Value;
            var cookieOpts = new Microsoft.AspNetCore.Http.CookieOptions { Path = "/", Expires = DateTimeOffset.UtcNow.AddYears(1) };
            Response.Cookies.Append("theme", string.IsNullOrEmpty(prefTheme) ? "light" : prefTheme, cookieOpts);
            Response.Cookies.Append("calendar", string.IsNullOrEmpty(prefCalendar) ? "Gregorian" : prefCalendar, cookieOpts);
            var prefLang = jwt.Claims.FirstOrDefault(c => c.Type == "Lang")?.Value;
            if (!string.IsNullOrEmpty(prefLang))
            {
                Response.Cookies.Append(
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
                        new Microsoft.AspNetCore.Localization.RequestCulture(prefLang)),
                    cookieOpts);
            }

            // Save JWT itself if needed later
            claims.Add(new Claim("JwtToken", model.Token));

            // 👉 Extract roles (token might use "role", "roles" or ClaimTypes.Role)
            var roleClaims = jwt.Claims.Where(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles");

            foreach (var role in roleClaims)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Value));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                });
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            var userProfile = new UserProfileViewModel
            {
                Employee = new EmployeeVM { Name = User.Identity?.Name ?? "" },
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value ?? User.Identity?.Name,
                EmployeeId = employeeIdClaim,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            };

            if (int.TryParse(employeeIdClaim, out var employeeId) && employeeId > 0)
            {
                try
                {
                    var response = await _employeeService.GetEmployeeAsync(employeeId);
                    if (response?.IsSuccess == true && response.Result is not null)
                    {
                        var employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response.Result));
                        if (employee is not null)
                        {
                            // An employee's manager is their department's manager.
                            if (employee.DepartmentId > 0)
                            {
                                var deptResp = await _departmentService.GetDepartmentAsync(employee.DepartmentId);
                                if (deptResp?.IsSuccess == true && deptResp.Result is not null)
                                {
                                    var dept = JsonConvert.DeserializeObject<Emp.Web.Dtos.DepartmentDto>(Convert.ToString(deptResp.Result));
                                    employee.Manager = dept?.ManagerName;
                                }
                            }
                            userProfile.Employee = employee;
                        }
                    }
                }
                catch
                {
                    // Keep the placeholder Employee built above.
                }
            }

            return View(userProfile);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var vm = new EditProfileVM();
            if (int.TryParse(User.FindFirst("EmployeeId")?.Value, out var employeeId) && employeeId > 0)
            {
                var response = await _employeeService.GetEmployeeAsync(employeeId);
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    var employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response.Result));
                    vm.Name = employee?.Name;
                    vm.Email = employee?.Email;
                    vm.Phone = employee?.Phone;
                    vm.Address = employee?.Address;
                }
            }
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileVM model)
        {
            var response = await _employeeService.UpdateMyProfileAsync(model.Phone, model.Address);
            if (response?.IsSuccess == true)
            {
                TempData["success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            TempData["error"] = response?.Message ?? "Could not update your profile.";
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(string theme, string calendar, string language)
        {
            theme = theme == "dark" ? "dark" : "light";
            calendar = calendar == "Hijri" ? "Hijri" : "Gregorian";
            language = language == "ar" ? "ar" : "en";

            await _accountService.UpdatePreferencesAsync(theme, calendar, language);

            var opts = new Microsoft.AspNetCore.Http.CookieOptions { Path = "/", Expires = DateTimeOffset.UtcNow.AddYears(1) };
            Response.Cookies.Append("theme", theme, opts);
            Response.Cookies.Append("calendar", calendar, opts);
            // Apply the chosen UI language immediately.
            Response.Cookies.Append(
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
                    new Microsoft.AspNetCore.Localization.RequestCulture(language)),
                opts);

            TempData["success"] = "Preferences saved.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordVM());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "The new password and confirmation do not match.");
                return View(model);
            }

            var response = await _accountService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);
            if (response?.IsSuccess == true)
            {
                // For a forced first-login change, sign out so the next login issues a token
                // without the MustChangePassword flag.
                if (User.HasClaim("MustChangePassword", "true"))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    _tokenProvider.ClearToken();
                    TempData["success"] = "Password changed. Please sign in again.";
                    return RedirectToAction(nameof(Login));
                }
                TempData["success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }
            ModelState.AddModelError("", response?.Message ?? "Could not change your password.");
            return View(model);
        }

        private List<string> GetRoleFromToken(string token)
        {
            if (token is not null)
            {
                // Extract roles from token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var roles = jwtToken.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role") // depends on how your API sets role claim
                    .Select(c => c.Value)
                    .ToList();
                return roles;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}


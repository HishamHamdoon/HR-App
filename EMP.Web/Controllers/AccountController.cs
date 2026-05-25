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
          public AccountController(HttpClient httpClient,IAccountService accountService,ITokenProvider tokenProvider,IEmployeeService employeeService)
        {
            _httpClient = httpClient;
            _accountService = accountService;
            _tokenProvider =tokenProvider;
            _employeeService = employeeService;

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


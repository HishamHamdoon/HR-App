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
            if (response.IsSuccess && response.Result != null)
            {
                var loginResponse = JsonConvert.DeserializeObject<Emp.Web.Dtos.Auth.LoginResponseDto>(response.Result.ToString());

                await SignIn(loginResponse); // Sign in with roles from JWT
                _tokenProvider.SetToken(loginResponse.Token);
                var roles = GetRoleFromToken(loginResponse.Token);
                if (roles.Contains("Employee"))
                {
                    return RedirectToAction("EmployeeDashboard", "Home");
                }
                    return RedirectToAction("Index", "Home");
            }

            // login failed  
            ViewBag.Errors = new List<string> { "Invalid login attempt." };
            return View(model);
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
            var response = await _httpClient.PostAsync("https://localhost:7031/api/Auth/register", content);

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
            // Clear authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("jwtToken"); // optional if you set JWT cookie


            // Or if you configured CookieAuthenticationDefaults
            // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear custom jwt cookie if you set one
            Response.Cookies.Delete("jwtToken");

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
            var claims = User.Claims.ToList(); // optional, for debugging
            var EmployeeId = User.FindFirst("EmployeeId")?.Value;
            var response =  await _employeeService.GetEmployeeAsync(employeeId:int.Parse(EmployeeId));
            var employee = JsonConvert.DeserializeObject<EmployeeVM>(Convert.ToString(response?.Result));
            var userProfile = new UserProfileViewModel
            {
                Employee=employee,
                Email = User.FindFirst(ClaimTypes.Email)?.Value
                        ?? User.FindFirst("email")?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value
                       ?? User.FindFirst("name")?.Value,
                EmployeeId = User.FindFirst("EmployeeId")?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            };

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


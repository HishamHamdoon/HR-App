using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    //[Authorize(Roles = "Admin")] // Only Admins can manage roles
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        public async Task<IActionResult> Index()
        {
            var responseString = await _roleService.GetAllRolesAsync(); // assume this returns JSON string
            //var rolesResponse = JsonConvert.DeserializeObject<ApiResponse<List<RoleDto>>>(responseString.Result.ToString());
            var rolesResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(responseString.Result.ToString());
            if (rolesResponse != null && rolesResponse.IsSuccess && rolesResponse.Result != null)
            {
                return View(rolesResponse.Result); // ✅ This is List<RoleDto>
            }
            return View(new List<RoleDto>());
        }
        //Create (GET)
        public IActionResult Create()
        {
            return View();
        }
        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            
            var response = await _roleService.CreateRoleAsync(roleName); // Call service to create
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create role");
            return View(roleName);
        }
        //Create (POST)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(string roleName)
        //{
        //    if (string.IsNullOrWhiteSpace(roleName))
        //    {
        //        ModelState.AddModelError("", "Role name cannot be empty.");
        //        return View();
        //    }

        //    if (await _roleManager.RoleExistsAsync(roleName))
        //    {
        //        ModelState.AddModelError("", $"Role '{roleName}' already exists.");
        //        return View();
        //    }

        //    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        //    if (result.Succeeded)
        //        return RedirectToAction(nameof(Index));

        //    foreach (var error in result.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return View();
        //}

        //// ✅ Delete
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(string roleId)
        //{
        //    var role = await _roleManager.FindByIdAsync(roleId);
        //    if (role == null)
        //        return NotFound();

        //    var result = await _roleManager.DeleteAsync(role);
        //    if (!result.Succeeded)
        //    {
        //        foreach (var error in result.Errors)
        //            ModelState.AddModelError("", error.Description);
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        //// ✅ Assign role to user (GET)
        //public IActionResult Assign()
        //{
        //    ViewBag.Users = _userManager.Users.ToList();
        //    ViewBag.Roles = _roleManager.Roles.ToList();
        //    return View();
        //}

        //// ✅ Assign role to user (POST)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Assign(string userId, string roleName)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null) return NotFound();

        //    if (!await _roleManager.RoleExistsAsync(roleName))
        //    {
        //        ModelState.AddModelError("", $"Role '{roleName}' does not exist.");
        //        return RedirectToAction(nameof(Assign));
        //    }

        //    var result = await _userManager.AddToRoleAsync(user, roleName);
        //    if (result.Succeeded)
        //        return RedirectToAction(nameof(Index));

        //    foreach (var error in result.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return RedirectToAction(nameof(Assign));
        //}

        //// ✅ View user roles
        //public async Task<IActionResult> UserRoles(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null) return NotFound();

        //    var roles = await _userManager.GetRolesAsync(user);
        //    ViewBag.User = user;
        //    return View(roles);
        //}
        public class RoleDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public class ApiResponse<T>
        {
            public bool IsSuccess { get; set; }
            public T Result { get; set; }
            public string Message { get; set; }
        }


    }
}

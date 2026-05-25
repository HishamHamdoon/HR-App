using Emp.Api.Dtos;
using Emp.Api.Services.IServices;
using Emp.Models.Models;
using Microsoft.AspNetCore.Identity;

namespace Emp.Api.Services
{
    public class RoleService: IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<ResponseDto> GetAllRolesAsync1()
        {
            var response = new ResponseDto();
            try
            {
                var roles = _roleManager.Roles.Select(r =>new { r.Name, r.Id }).ToList();
                if (roles.Any())
                {
                    response.IsSuccess = true;
                    response.Result = roles;
                    response.Message = "";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Result = roles;
                    response.Message = "No data found.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            return result.Succeeded;
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<ResponseDto> GetAllRolesAsync()
        {
            var response = new ResponseDto();
            try
            {
                var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                response.IsSuccess = true;
                response.Result = roles;
                response.Message = "";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

    }
}

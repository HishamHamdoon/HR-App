using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class RoleService : IRoleService
    {
        private readonly IBaseService _baseService;
        public RoleService(IBaseService baseService)
        {
           _baseService = baseService;
        }

        public Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto> CreateRoleAsync(string roleName)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"https://localhost:7031/api/Roles",
                Data=roleName
            });
        }

        public async Task<ResponseDto> GetAllRolesAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/Roles"

            });
        }

        public Task<IList<string>> GetUserRolesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            throw new NotImplementedException();
        }
    }
}

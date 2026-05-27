
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IRoleService
    {
        Task<ResponseDto> GetAllRolesAsync();
        Task<ResponseDto> CreateRoleAsync(string roleName);
        Task<ResponseDto> DeleteRoleAsync(string roleName);
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);
    }
}

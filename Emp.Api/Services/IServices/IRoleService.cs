using Emp.Api.Dtos;

namespace Emp.Api.Services.IServices
{
    public interface IRoleService
    {
        Task<ResponseDto> GetAllRolesAsync();
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleName);
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);
    }
}

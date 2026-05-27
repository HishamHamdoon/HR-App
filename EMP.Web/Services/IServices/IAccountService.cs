using Emp.Web.Models.Dtos;
using Emp.Web.Dtos.Auth;

namespace EMP.Web.Services.IServices
{
    public interface IAccountService
    {
        Task<ResponseDto> RegisterAsync(RegisterationRequest loginRequestDto);
        Task<ResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<ResponseDto> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<ResponseDto> UpdatePreferencesAsync(string? theme, string? calendar, string? language);
    }
}

using Emp.Web.Dtos.Auth;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Identity.Data;

namespace EMP.Web.Services
{
    public class AccountService : IAccountService
    {
        private readonly IBaseService _baseService;
        public AccountService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Auth/login/",
                Data= loginRequest
            });
        }

        public Task<ResponseDto> RegisterAsync(RegisterationRequest loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Auth/change-password",
                Data = new { CurrentPassword = currentPassword, NewPassword = newPassword }
            });
        }

        public async Task<ResponseDto> UpdatePreferencesAsync(string? theme, string? calendar, string? language)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Auth/preferences",
                Data = new { Theme = theme, Calendar = calendar, Language = language }
            });
        }
    }
}

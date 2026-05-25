using Emp.Web.Dtos.Auth;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;
        public AuthService(IBaseService baseService)
        {
            _baseService=baseService;
        }
        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Auth/register",
                Data=registerDto
            });
        }
    }
}

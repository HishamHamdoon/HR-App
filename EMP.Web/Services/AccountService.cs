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
                Url = "https://localhost:7031/api/Auth/login/",
                Data= loginRequest
            });
        }

        public Task<ResponseDto> RegisterAsync(RegisterationRequest loginRequestDto)
        {
            throw new NotImplementedException();
        }
    }
}

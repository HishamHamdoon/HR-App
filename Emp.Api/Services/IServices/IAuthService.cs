using Emp.Api.Dtos;
using Emp.Api.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emp.Api.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto> Register(RegisterDto registerDto);
        Task<LoginResponseDto?> Login(LoginRequestDto loginRequestDto);
        Task<ResponseDto?> AssignRole(string email,string roleName);
    }
}

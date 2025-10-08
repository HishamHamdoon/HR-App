

using Emp.Web.Dtos.Auth;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto> RegisterAsync(RegisterDto registerDto);
    }
}

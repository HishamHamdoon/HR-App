using Emp.Web.Models.Dtos;
using EMP.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IBaseService
    {
        Task<ResponseDto?> SendAsync(RequestDto requestDto);
    }
}

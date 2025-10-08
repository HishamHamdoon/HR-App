using Emp.Web.Models.Dtos;
using EMP.Web.Models;

namespace EMP.Web.Services.IServices
{
    public interface ISectionService
    {
        Task<ResponseDto> GetSectionsAsync();
    }
}

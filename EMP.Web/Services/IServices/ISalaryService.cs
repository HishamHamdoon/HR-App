using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using EMP.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ISalaryService
    {
        Task<ResponseDto> GetSalariesAsync();
        Task<ResponseDto> CreateSalaryAsync(CreateSalaryDto createSalaryDto);
        Task<ResponseDto> GetSalaryAsync(int id);
        Task<ResponseDto> DeleteSalaryAsync(int id);
        Task<ResponseDto> UpdateSalaryAsync(SalaryDto salaryDto);
    }
}

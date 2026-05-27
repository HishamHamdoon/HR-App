using Emp.Web.Models.Dtos;
using EMP.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ISettingsService
    {
        Task<ResponseDto> GetAsync();
        Task<ResponseDto> UpdateAsync(CompanySettingsDto dto);

        /// <summary>Convenience: fetch settings deserialized, or sensible defaults.</summary>
        Task<CompanySettingsDto> GetSettingsAsync();
    }
}

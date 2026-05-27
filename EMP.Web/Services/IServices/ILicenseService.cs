using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ILicenseService
    {
        Task<ResponseDto> GetStatusAsync();
        Task<ResponseDto> ActivateAsync(string key);
        Task<ResponseDto> GenerateAsync(string type, int months);
    }
}

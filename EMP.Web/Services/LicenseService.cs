using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly IBaseService _baseService;

        public LicenseService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> GetStatusAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/License/status"
            });
        }

        public async Task<ResponseDto> ActivateAsync(string key)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/License/activate",
                Data = new { Key = key }
            });
        }

        public async Task<ResponseDto> GenerateAsync(string type, int months)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/License/generate?type={type}&months={months}"
            });
        }
    }
}

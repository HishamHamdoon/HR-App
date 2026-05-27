using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Newtonsoft.Json;

namespace EMP.Web.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IBaseService _baseService;

        public SettingsService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> GetAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Settings"
            });
        }

        public async Task<ResponseDto> UpdateAsync(CompanySettingsDto dto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"{SD.ApiBaseUrl}/api/Settings",
                Data = dto
            });
        }

        public async Task<CompanySettingsDto> GetSettingsAsync()
        {
            try
            {
                var response = await GetAsync();
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    return JsonConvert.DeserializeObject<CompanySettingsDto>(Convert.ToString(response.Result))
                           ?? new CompanySettingsDto();
                }
            }
            catch { /* fall back to defaults */ }
            return new CompanySettingsDto();
        }
    }
}

using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class SetupService : ISetupService
    {
        private readonly IBaseService _baseService;
        public SetupService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto> GetCountriesList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/countries-dp-list"
            });
        }

        public async Task<ResponseDto> GetDepartmentsList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/departments-dp-list"
            });
        }

        public async Task<ResponseDto> GetEmployeesList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/employee-dp-list"
            });
        }

        public async Task<ResponseDto> GetJobTitleesList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/jot-title-dp-list"
            });
        }

        public async Task<ResponseDto> GetLeaveTypesList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/leave-type-dp-list"
            });
        }

        public async Task<ResponseDto> GetSectionsList()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Setup/sections-dp-list"
            });
        }
    }
}

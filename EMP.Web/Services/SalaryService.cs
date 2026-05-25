using DocumentFormat.OpenXml.Office2010.Excel;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using static Emp.Web.Utility.SD;

namespace EMP.Web.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IBaseService _baseService;
        public SalaryService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateSalaryAsync(CreateSalaryDto createSalaryDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Salary/CreateSalary",
                Data=createSalaryDto
            });
        }

      

        public async Task<ResponseDto> GetSalaryAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Salary/{id}"
            });
        }

        public async Task<ResponseDto> GetSalariesAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Salary"
            });
        }

        public async Task<ResponseDto> UpdateSalaryAsync(SalaryDto salaryDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"{SD.ApiBaseUrl}/api/Salary/UpdateSalary",
                Data=salaryDto
            });
        }

        public async Task<ResponseDto> DeleteSalaryAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Delete,
                Url = $"{SD.ApiBaseUrl}/api/Salary/{id}"
            });
        }
    }
}

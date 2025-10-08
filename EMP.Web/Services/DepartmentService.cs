using DocumentFormat.OpenXml.Office2010.Excel;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using static Emp.Web.Utility.SD;

namespace EMP.Web.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IBaseService _baseService;
        public DepartmentService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateDepartmentsAsync(DepartmentCreateDto departmentDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.DepartmentAPIUrl}",
                Data=departmentDto
            });
        }

      

        public async Task<ResponseDto> GetDepartmentAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.DepartmentAPIUrl}/{id}"
            });
        }

        public async Task<ResponseDto> GetDepartmentsAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.DepartmentAPIUrl
            });
        }

        public async Task<ResponseDto> GetSectionsByDepartmentAsync(int departmentId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.DepartmentAPIUrl}/{departmentId}"
             });
        }
    }
}

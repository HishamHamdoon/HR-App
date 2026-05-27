using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class SectionService : ISectionService
    {
        private readonly IBaseService _baseService;
        public SectionService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto> GetSectionsAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.SectionsAPIUrl
            });
        }

        public async Task<ResponseDto> CreateSectionAsync(string name, int departmentId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = SD.SectionsAPIUrl,
                Data = new { Name = name, DepartmentId = departmentId }
            });
        }
    }
}

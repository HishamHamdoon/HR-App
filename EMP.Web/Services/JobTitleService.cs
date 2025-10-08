using Emp.Web.Dtos.JobTitle;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly IBaseService _baseService;
        public JobTitleService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateJobTitlesAsync(JobTitleCreateDto jobTitleCreateDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = SD.JobTitleAPIUrl,
                Data=jobTitleCreateDto
            });
        }
        public async Task<ResponseDto> UpdateJobTitlesAsync(JobTitleCreateDto jobTitleCreateDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"{SD.JobTitleAPIUrl}/{jobTitleCreateDto.Id}",
                Data = jobTitleCreateDto
            });
        }
        public async Task<ResponseDto> GetJobTitleAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.JobTitleAPIUrl}/{id}"
            });
        }
        public async Task<ResponseDto> DeleteJobTitleAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Delete,
                Url = $"{SD.JobTitleAPIUrl}/{id}"
            });
        }

        public async Task<ResponseDto> GetJobTitlesAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.JobTitleAPIUrl
            });
        }
   
    }
}

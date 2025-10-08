using Emp.Web.Dtos.JobTitle;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IJobTitleService
    {
        Task<ResponseDto> GetJobTitlesAsync();
        Task<ResponseDto> GetJobTitleAsync(int id);
        Task<ResponseDto> DeleteJobTitleAsync(int id);
        Task<ResponseDto> CreateJobTitlesAsync(JobTitleCreateDto jobTitleCreateDto);
        Task<ResponseDto> UpdateJobTitlesAsync(JobTitleCreateDto jobTitleCreateDto);
    }
}

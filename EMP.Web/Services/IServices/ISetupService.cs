
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ISetupService
    {
        Task<ResponseDto> GetEmployeesList();
        Task<ResponseDto> GetLeaveTypesList();
        Task<ResponseDto> GetDepartmentsList();
        Task<ResponseDto> GetSectionsList();
        Task<ResponseDto> GetCountriesList();
        Task<ResponseDto> GetJobTitleesList();
    }
}


using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IDepartmentService
    {
        Task<ResponseDto> GetDepartmentsAsync();
        Task<ResponseDto> CreateDepartmentsAsync(DepartmentCreateDto departmentDto);
        Task<ResponseDto> UpdateDepartmentAsync(DepartmentCreateDto departmentDto);
        Task<ResponseDto> GetDepartmentAsync(int id);
        Task<ResponseDto> GetSectionsByDepartmentAsync(int departmentId);
    }
}

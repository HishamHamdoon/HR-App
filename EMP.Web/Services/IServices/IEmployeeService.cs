//using Emp.Api.Dtos.Employee;
//using Emp.Api.Dtos;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface IEmployeeService
    {
        Task<ResponseDto> GetEmployeesAsync(int page,int pageSize);
        Task<ResponseDto> GetEmployeeAsync(int employeeId);
        Task<ResponseDto> CreateEmployeeAsync(EmployeeCreateDto employeeCreateDto);
        Task<ResponseDto> DeleteEmployeeAsync(int employeeId);
        Task<ResponseDto> EditEmployeeAsync(EmployeeCreateDto employeeUpdateDto);
        Task<ResponseDto> ActiveDeActiveEmployee(int employeeId);
        Task<ResponseDto> GetDashboardAsync();
        Task<ResponseDto> GetDashboardChartsAsync();
        Task<ResponseDto> SetManager(int employeeId, int managerId);
        Task<ResponseDto> GetManagerNameAsync(int employeeId);
        Task<ResponseDto> TerminateEmployeeAsync(int id,TerminationDto terminationDto);

    }
}

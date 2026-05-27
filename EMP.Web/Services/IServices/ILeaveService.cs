//using Emp.Api.Dtos.Employee;
//using Emp.Api.Dtos.Leave;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ILeaveService
    {
        Task<ResponseDto> GetLeavesAsync(int page,int pageSize);
        Task<ResponseDto> GetLeaveAsync(int leaveId);
        Task<ResponseDto> CreateLeaveAsync(CreateLeaveDto createLeaveDto);
        Task<ResponseDto> DeleteLeaveAsync(int leaveId);
        Task<ResponseDto> EditLeaveAsync(UpdateLeaveDto updateLeaveDto);
        Task<ResponseDto> ActiveDeActiveLeave(int leaveId);
        Task<ResponseDto> GetLeavesByEmployeeIdAsync(int employeeId);
        Task<ResponseDto> GetLeavesByManagerAsync(int managerId);
        Task<ResponseDto> GetLeaveBalanceAsync(int employeeId);
        Task<ResponseDto> DecideLeaveAsync(int leaveId, string status, string? note = null);
    }
}

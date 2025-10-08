using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ILeavesTypeService
    {
        Task<ResponseDto> GetLeavesTypeAsync();
        Task<ResponseDto> CreateLeavesTypeAsync(CreateLeaveTypesDto createLeaveTypesDto);
        Task<ResponseDto> DeleteLeavesTypeAsync(int id);
        Task<ResponseDto> UpdateLeavesTypeAsync(UpdateLeaveTypesDto updateLeaveTypesDto);
        Task<ResponseDto> GetLeavesTypeAsync(int id);

    }
}

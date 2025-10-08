using DocumentFormat.OpenXml.Office2010.Excel;
using Emp.Api.Dtos.Leave;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class LeaveTypeService : ILeavesTypeService
    {
        private readonly IBaseService _baseService;
        public LeaveTypeService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateLeavesTypeAsync(Emp.Web.Dtos.CreateLeaveTypesDto createLeaveTypesDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"https://localhost:7031/api/LeaveTypes",
                Data=createLeaveTypesDto
            });
        }

        public async Task<ResponseDto> DeleteLeavesTypeAsync(int id)
        {
            var response = await _baseService.SendAsync(new RequestDto
            {
                ApiType= SD.ApiType.Delete,
                Url = $"https://localhost:7031/api/LeaveTypes/{id}",
            });
            return response;
        }

        public async Task<ResponseDto> GetLeavesTypeAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/LeaveTypes"
            });
        }

        public async Task<ResponseDto> GetLeavesTypeAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/LeaveTypes/{id}",
            });
        }

        //public async Task<ResponseDto> UpdateLeavesTypeAsync(Emp.Web.Dtos.CreateLeaveTypesDto createLeaveTypesDto)
        //{
           
        //}

        public async Task<ResponseDto> UpdateLeavesTypeAsync(Emp.Web.Dtos.UpdateLeaveTypesDto updateLeaveTypesDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"https://localhost:7031/api/LeaveTypes/{updateLeaveTypesDto.Id}",
                Data = updateLeaveTypesDto
            });
        }
    }
}

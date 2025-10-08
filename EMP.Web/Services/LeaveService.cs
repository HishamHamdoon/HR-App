using Emp.Api.Models;
using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using System.Net.Http.Headers;
using static Emp.Web.Utility.SD;

namespace EMP.Web.Services.IServices
{
    public class LeaveService : ILeaveService
    {
        private readonly IBaseService _baseService;
        public LeaveService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public Task<ResponseDto> ActiveDeActiveLeave(int leaveId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto> CreateLeaveAsync(CreateLeaveDto createLeaveDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"https://localhost:7031/api/Leaves",
                Data = createLeaveDto,
                ContentType=SD.ContentType.MultiPartFormData
            });
        }
        //public async Task<ResponseDto?> CreateLeaveAsync(CreateLeaveDto dto)
        //{
        //    using var form = new MultipartFormDataContent();

        //    // Required fields
        //    form.Add(new StringContent(dto.EmployeeId.ToString()), "EmployeeId");
        //    form.Add(new StringContent(dto.LeavesTypeId.ToString()), "LeavesTypeId");
        //    form.Add(new StringContent(dto.StartDate.ToString("yyyy-MM-dd")), "StartDate");

        //    if (dto.EndDate.HasValue)
        //        form.Add(new StringContent(dto.EndDate.Value.ToString("yyyy-MM-dd")), "EndDate");

        //    if (!string.IsNullOrEmpty(dto.Note))
        //        form.Add(new StringContent(dto.Note), "Note");

        //    // File
        //    if (dto.Attachment != null)
        //    {
        //        var streamContent = new StreamContent(dto.Attachment.OpenReadStream());
        //        streamContent.Headers.ContentType = new MediaTypeHeaderValue(dto.Attachment.ContentType);
        //        form.Add(streamContent, "Attachment", dto.Attachment.FileName);
        //    }

        //    var requestDto = new RequestDto
        //    {
        //        ApiType = ApiType.Post,
        //        Url = "https://localhost:7031/api/Leaves/", // adjust URL
        //        Data = form   // ✅ send as multipart
        //    };

        //    return await _baseService.SendAsync(requestDto);
        //}

        public async Task<ResponseDto> DeleteLeaveAsync(int leaveId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Delete,
                Url = $"https://localhost:7031/api/Leaves/{leaveId}",
            });
        }

        public async Task<ResponseDto> EditLeaveAsync(UpdateLeaveDto updateLeaveDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"https://localhost:7031/api/Leaves",
                Data = updateLeaveDto,
                //ContentType = SD.ContentType.MultiPartFormData

            });
        }

        public async Task<ResponseDto> GetLeaveAsync(int leaveId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/Leaves/{leaveId}"
            });
        }

        public async Task<ResponseDto> GetLeavesAsync(int page, int pageSize)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url= $"https://localhost:7031/api/Leaves?page={page}&pageSize={pageSize}"
            });
        }

        public async Task<ResponseDto> GetLeavesByEmployeeIdAsync(int employeeId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/Leaves/get-leaves-by-employeeId/{employeeId}"
            });
        }

        public async Task<ResponseDto> GetLeavesByManagerAsync(int managerId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"https://localhost:7031/api/Leaves/get-leaves-by-managerId/{managerId}"
            });
        }



        //public async Task<ResponseDto> ActiveDeActiveEmployee(int employeeId)
        //{

        //        return await _baseService.SendAsync(new RequestDto
        //        {
        //            ApiType = SD.ApiType.Post,
        //            Url = SD.EmployeeAPIUrl + "/active-deactive-employee/" + employeeId
        //        });

        //}

        //public async Task<ResponseDto> CreateEmployeeAsync(EmployeeCreateDto employeeCreateDto)
        //{
        //    if (employeeCreateDto == null)
        //    {
        //        throw new ArgumentNullException(nameof(employeeCreateDto));
        //    }
        //    else
        //    {
        //        return await _baseService.SendAsync(new RequestDto
        //        {
        //            ApiType = SD.ApiType.Post,
        //            Url = SD.EmployeeAPIUrl,
        //            Data = employeeCreateDto
        //        });
        //    }
        //}

        //public Task<ResponseDto> CreateEmployeeAsync(Emp.Api.Dtos.Employee.EmployeeCreateDto employeeCreateDto)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<ResponseDto> DeleteEmployeeAsync(int employeeId)
        //{
        //    return await _baseService.SendAsync(new RequestDto
        //    {
        //        ApiType = SD.ApiType.Delete,
        //        Url = SD.EmployeeAPIUrl + "/" + employeeId
        //    });
        //}

        //public async Task<ResponseDto> EditEmployeeAsync(EmployeeCreateDto employeeUpdateDto)
        //{
        //    if (employeeUpdateDto == null)
        //    {
        //        throw new ArgumentNullException(nameof(employeeUpdateDto));
        //    }
        //    else
        //    {
        //        return await _baseService.SendAsync(new RequestDto
        //        {
        //            ApiType = SD.ApiType.Put,
        //            Url = SD.EmployeeAPIUrl+ "/update-employee/"+employeeUpdateDto.Id,
        //            Data = employeeUpdateDto
        //        });
        //    }
        //}

        //public async Task<ResponseDto> GetEmployeeAsync(int employeeId)
        //{
        //    return await _baseService.SendAsync(new RequestDto
        //    {
        //        ApiType = SD.ApiType.Get,
        //        Url = SD.EmployeeAPIUrl+"/"+employeeId
        //    });
        //}

        //public async Task<ResponseDto> GetEmployeesAsync(int page,int pageSize)
        //{
        //    return await _baseService.SendAsync(new RequestDto
        //    {
        //        ApiType = SD.ApiType.Get,
        //        Url = $"{SD.EmployeeAPIUrl}?page={page}&pageSize={pageSize}"
        //    });
        //}
    }
}

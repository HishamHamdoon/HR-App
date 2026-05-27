using Emp.Web.Dtos;
using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using static Emp.Web.Utility.SD;

namespace EMP.Web.Services.IServices
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IBaseService _baseService;
        public EmployeeService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> ActiveDeActiveEmployee(int employeeId)
        {
            return await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.Post,
                    Url = SD.EmployeeAPIUrl + "/active-deactive-employee/" + employeeId
                });
        }

        public async Task<ResponseDto> CreateEmployeeAsync(EmployeeCreateDto employeeCreateDto)
        {
            if (employeeCreateDto == null)
            {
                throw new ArgumentNullException(nameof(employeeCreateDto));
            }
            else
            {
                return await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.Post,
                    Url = SD.EmployeeAPIUrl,
                    Data = employeeCreateDto
                });
            }
        }
        public async Task<ResponseDto> DeleteEmployeeAsync(int employeeId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Delete,
                Url = SD.EmployeeAPIUrl + "/" + employeeId
            });
        }

        public async Task<ResponseDto> GetByManagerAsync(int managerId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.EmployeeAPIUrl}/by-manager/{managerId}"
            });
        }

        public async Task<ResponseDto> UpdateMyProfileAsync(string? phone, string? address)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = $"{SD.EmployeeAPIUrl}/my-profile",
                Data = new { Phone = phone, Address = address }
            });
        }

        public async Task<ResponseDto> EditEmployeeAsync(EmployeeCreateDto employeeUpdateDto)
        {
            if (employeeUpdateDto == null)
            {
                throw new ArgumentNullException(nameof(employeeUpdateDto));
            }
            else
            {
                return await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.Put,
                    Url = SD.EmployeeAPIUrl+ "/update-employee/"+employeeUpdateDto.Id,
                    Data = employeeUpdateDto
                });
            }
        }

       
        public async Task<ResponseDto> GetDashboardAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/dashboard-counts"
            });
        }

        public async Task<ResponseDto> GetDashboardChartsAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/dashboard-charts"
            });
        }

        public async Task<ResponseDto> GetEmployeeAsync(int employeeId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.EmployeeAPIUrl+"/"+employeeId
            });
        }

        public async Task<ResponseDto> GetEmployeesAsync(int page,int pageSize)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.EmployeeAPIUrl}?page={page}&pageSize={pageSize}"
            });
        }

        public async Task<ResponseDto> GetManagerNameAsync(int employeeId)
        {
           return await _baseService.SendAsync(new RequestDto {
                ApiType = SD.ApiType.Get,
                 Url = $"{SD.EmployeeAPIUrl}/GetManagerName/{employeeId}"
                });
        }

        public async Task<ResponseDto> TerminateEmployeeAsync(int id,TerminationDto terminationDto )
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Patch,
                Url = $"{SD.EmployeeAPIUrl}/terminate/{id}",
                Data=terminationDto
            });
        }

        public async Task<ResponseDto> ReactivateEmployeeAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Patch,
                Url = $"{SD.EmployeeAPIUrl}/reactivate/{id}"
            });
        }

        public async Task<ResponseDto> GetTerminationsAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.EmployeeAPIUrl}/terminations"
            });
        }
    }
}

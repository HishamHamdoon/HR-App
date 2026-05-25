using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public SetupController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        /// <summary>
        /// This function for employees dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("employee-dp-list")]
        public ResponseDto GetEmployeesList()
        {
            var response = new ResponseDto();
            try
            {
                var employees = _dbContext.Employees.Select(x => new { x.Id, x.Name }).ToList();
                response.IsSuccess = true;
                response.Result = employees;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
        /// <summary>
        /// This function for Leave Type dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("leave-type-dp-list")]
        public ResponseDto GetLeaveTypesList()
        {
            var response = new ResponseDto();

            try
            {
                // 1. Load data into memory first (LINQ to Entities ends here)
                var result =  _dbContext.LeavesTypes
                    .AsNoTracking()
                    .ToList(); // ✅ EF Core can execute this

                // 2. Now project to anonymous type using LINQ to Objects
                var leaveTypes = result
                    .Select(x => new Dictionary<string, object>
                    {
                        ["Id"] = x.Id,
                        ["Name"] = x.Name
                    })
                    .ToList(); // ✅ safe for serialization

                response.IsSuccess = true;
                response.Result = leaveTypes;
                response.Message = "";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// This function for department dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("departments-dp-list")]
        public ResponseDto GetDepartmentsList()
        {
            var response = new ResponseDto();
            try
            {
                var departments = _dbContext.Departments.Select(x => new { x.Id, x.Name }).ToList();
                response.IsSuccess = true;
                response.Result = departments;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
        /// <summary>
        /// This function for section dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("sections-dp-list")]
        public async Task<ResponseDto> GetSectionsList()
        {
            var response = new ResponseDto();
            try
            {
                var sections = _dbContext.Sections.Select(x => new { x.Id, x.Name }).ToList();
                response.IsSuccess = true;
                response.Result = sections;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
        /// <summary>
        /// This function for countries dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("countries-dp-list")]
        public async Task<ResponseDto> GetCountriesList()
        {
            var response = new ResponseDto();
            try
            {
                var countries = _dbContext.Countries.Select(x => new { x.Id, x.Name }).ToList();
                response.IsSuccess = true;
                response.Result = countries;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
        /// <summary>
        /// This function for JobTitle dropdown list
        /// </summary>
        /// <returns></returns>
        [HttpGet("jot-title-dp-list")]
        public async Task<ResponseDto> GetJobTitlesList()
        {
            var response = new ResponseDto();
            try
            {
                var jobTitles = _dbContext.JobTitles.Select(x => new { x.Id, x.Title }).ToList();
                response.IsSuccess = true;
                response.Result = jobTitles;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}

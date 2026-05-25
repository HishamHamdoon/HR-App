using AutoMapper;
using Azure;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Employee;
using Emp.Api.Dtos.Leave;
using Emp.Api.Dtos.Vacation;
using Emp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public LeaveTypesController(AppDbContext dbContext, IMapper mapper) 
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all leave types as list 
        /// </summary>
        /// <returns>ResponseDto which conatins a list of leave type in Result prop</returns>
        [HttpGet]
        public async Task<ResponseDto> GetAll()
        {
            ResponseDto response = new ResponseDto();
            try
            {
                var LeaveTypesModeldel = await _dbContext.LeavesTypes.ToListAsync();
                var leavesTypeDtoList = _mapper.Map<List<LeaveTypesViewDto>>(LeaveTypesModeldel)
                                        ?? new List<LeaveTypesViewDto>();
                response.Message = "";
                response.Result = leavesTypeDtoList;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Message=ex.Message;
                response.IsSuccess=false;
                response.Result=false;
                return response;
            }
        }
        /// <summary>
        /// Get single leave types based on id
        /// </summary>
        /// <param name="id">leave type id</param>
        /// <returns>ResponseDto which conatins single leave type in Result prop</returns>
        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(int id)
        {
            ResponseDto response = new ResponseDto();
            try
            {
                var LeaveTypeModel = await _dbContext.LeavesTypes.FirstOrDefaultAsync(x => x.Id == id);

                if (LeaveTypeModel == null)
                {
                    response.Message = "Leave type not found";
                    response.IsSuccess = false;
                    response.Result = null;
                    return response;
                }
                else
                {
                    response.Message = "";
                    response.Result = _mapper.Map<LeaveTypesViewDto>(LeaveTypeModel);
                    response.IsSuccess = true;
                    return response;
                }
            }
            catch (Exception)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred";
                response.Result = null;
                return response;
            }
           
        }

        /// <summary>
        /// create new Leave Type based on Create dto object
        /// </summary>
        /// <param name="createLeaveTypesDto">Dto object to create</param>
        /// <returns>ResponseDto which conatins created leaveType in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] CreateLeaveTypesDto createLeaveTypesDto)
        {
            ResponseDto response=new ResponseDto();
            try
            {
                var LeavetypeModel = _mapper.Map<LeavesType>(createLeaveTypesDto);
                await _dbContext.LeavesTypes.AddAsync(LeavetypeModel);
                await _dbContext.SaveChangesAsync();
                var resultDto = _mapper.Map<LeaveTypesViewDto>(LeavetypeModel);
                response.Message = "Leave Type created successfully";
                response.IsSuccess = true;
                response.Result = resultDto;
                return response;
            }
            catch (Exception ex)
            {
                response.Message=ex.Message;
                response.IsSuccess=false;
                response.Result = null;
                return response;
            }
            
        }
        /// <summary>
        /// Update LeaveType based on 
        /// </summary>
        /// <param name="id">LeaveTypeId which we need to update </param>
        /// <param name="updateLeaveTypesDto">new object of LeaveTypeDto</param>
        /// <returns>new object of ResponseDto which contains Updated object in Result prop </returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(int id,[FromBody] UpdateLeaveTypesDto updateLeaveTypesDto)
        {
            ResponseDto response=new ResponseDto();
            try
            {
                var leaveType = await _dbContext.LeavesTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (leaveType != null)
                {
                    var mappedmodel = _mapper.Map<LeavesType>(updateLeaveTypesDto);
                    _dbContext.Update(mappedmodel);
                    await _dbContext.SaveChangesAsync();
                    LeaveTypesViewDto resultDto = _mapper.Map<LeaveTypesViewDto>(leaveType);
                    response.Result = resultDto;
                    response.IsSuccess = true;
                    response.Message = "Leave type updated successfully";
                    return response;
                }
                else
                {
                    response.Message= "Leave type not found";
                    response.Result= null;
                    response.IsSuccess=false;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }            
        }
        /// <summary>
        /// Delete LeaveType based on supplied Id
        /// </summary>
        /// <param name="id">LeaveType to delete</param>
        /// <returns>ResponseDto whcih contains deleted LeaveType in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(int id)
        {
            ResponseDto response = new ResponseDto();
            try
            {
                var leaveTypeModel = await _dbContext.LeavesTypes.FindAsync(id);
                if (leaveTypeModel != null)
                {
                    _dbContext.LeavesTypes.Remove(leaveTypeModel);
                    await _dbContext.SaveChangesAsync();
                    response.Result =_mapper.Map<LeaveTypesViewDto>(leaveTypeModel);
                    response.Message = "Leave Type Deleted successfully";
                    response.IsSuccess = true;
                    return response;
                }
                else
                {
                    response.Result = null;
                    response.Message = "Leave Type not found";
                    response.IsSuccess = false;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Message =ex.Message;
                response.IsSuccess = false;
                return response;
            }
              
        }
    }
}

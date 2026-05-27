using AutoMapper;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.JobTitleDto;
using Emp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JobTitleController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        //private readonly ResponseDto _response;

        public JobTitleController(AppDbContext dbContext, IMapper mapper) 
        {
            _dbContext = dbContext;
            _mapper = mapper;
            
        }
        // GET: api/<JobTitlesController>
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> JobTitle()
        {
            ResponseDto _response = new ResponseDto();
            var JobtitleModeldel = await _dbContext.JobTitles.ToListAsync();
            _response.Result = _mapper.Map<List<JobTitleViewDto>>(JobtitleModeldel) ?? new List<JobTitleViewDto>();
            _response.Message = "";
            _response.IsSuccess = true;
            return Ok(_response);
            
        }

        // GET api/<JobTitlesController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            ResponseDto _response = new ResponseDto();
            var JobTitleModel = await _dbContext.JobTitles.Include(e=>e.Employees).FirstOrDefaultAsync(x => x.Id == id);

            if (JobTitleModel == null)
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "JobTitle not found"
                }
                );
            }
            else
            {
                var response = _mapper.Map<JobTitleViewDto>(JobTitleModel);
                return Ok(new ResponseDto
                {
                    IsSuccess = response is not null,
                    Message = "",
                    Result=response
                });

            }
        }

        // POST api/<JobTitlesController>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async  Task<ResponseDto> JobTitle([FromBody] JobTitleCreateDto createDto)
        {
            ResponseDto response = new ResponseDto();
           
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Message = "Error";
                response.Result = ModelState.Values.SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
                return response;
            }
            else
            {
                var JobTitleModel = _mapper.Map<JobTitle>(createDto);
                await _dbContext.JobTitles.AddAsync(JobTitleModel);
                await _dbContext.SaveChangesAsync();
                response.IsSuccess = true;
                response.Result = new { JobTitleModel.Id, JobTitleModel.Title };
                response.Message = "Job title created Successfully";
                return response;
            }
        }
        
        // PUT api/<JobTitlesController>/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ResponseDto> JobTitle(int id,[FromBody] JobTitleUpdateDto JobTitlesUpdateDto)
        {
            ResponseDto response = new ResponseDto();
            var exists = await _dbContext.JobTitles.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = "Job title not found";
                return response;
            }
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Result = ModelState.Values.SelectMany(error => error.Errors).Select(e => e.ErrorMessage).ToList();
                response.Message = "Error";
                return response;
            }
            var mappedmodel = _mapper.Map<JobTitle>(JobTitlesUpdateDto);
            mappedmodel.Id = id; // route id is authoritative
            _dbContext.Update(mappedmodel);
            await _dbContext.SaveChangesAsync();
            response.IsSuccess = true;
            response.Result = new { mappedmodel.Id, mappedmodel.Title };
            response.Message = "Job title updated successfully";
            return response;
        }

        // DELETE api/<JobTitlesController>/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        //[Route("job-titles-delete")]
        public async Task<ResponseDto> JobTitle(int id)
        {
            ResponseDto response = new ResponseDto();
            JobTitle JobTitleModel = await _dbContext.JobTitles.FindAsync(id);
            if(JobTitleModel != null)
            {
                _dbContext.JobTitles.Remove(JobTitleModel);
                _dbContext.SaveChanges();
                response.IsSuccess= true;
                response.Result = true;
                response.Message = "Job title is deleted successfully";
                return response;
            }
            response.IsSuccess = false;
            response.Result = false;
            response.Message = "Job title not found";
            return response;
              
        }
    }
}

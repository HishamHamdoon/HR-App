using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Bibliography;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Models;
using Emp.Api.Dtos.Section;
using Emp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SectionsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public SectionsController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all sections 
        /// </summary>
        /// <returns>ResponseDto object which contains a list of section in Result prop</returns>
        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            ResponseDto response = new ResponseDto();
            try
            {
                var sectionList = _dbContext.Sections.Include(e => e.Department).Where(e => e.DepartmentId != null).ToList();
                response.Result = _mapper.Map<List<Section>>(sectionList ?? new List<Section>());
                response.IsSuccess = true;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.IsSuccess = false;
                response.Message = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// Get single section based on supplied id
        /// </summary>
        /// <param name="id">section Id</param>
        /// <returns>ResponseDto object which contains a single section in Result prop</returns>
        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(int id)
        {
            ResponseDto response = new();
            try
            {
                var sections = await _dbContext.Sections
                                   .Where(s => s.DepartmentId == id)
                                   .Select(s => new { s.Id, s.Name })
                                   .ToListAsync();

                //var sectionModel = await _dbContext.Sections.Include(e=>e.Department).Where(e=>e.DepartmentId!= null).SingleOrDefaultAsync(x => x.Id == id);
                if (sections == null)
                {
                    response.Result = null;
                    response.Message = "Section not found";
                    response.IsSuccess = false;
                    return response;
                }
                else
                {
                    response.Result = _mapper.Map<SectionViewDto>(sections);
                    response.Message = "";
                    response.IsSuccess = true;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.Message= ex.Message;
                response.IsSuccess=false;
                return response;
            }
        }
        /// <summary>
        /// Create new Section based on supplied object of SectionCreateDto
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns>ResponseDto whcih contains createdion in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] SectionCreateDto createDto)
        {
            ResponseDto response = new ();
            try
            {
                if (ModelState.IsValid)
                {
                    Section sectionModel = _mapper.Map<Section>(createDto);
                    await _dbContext.Sections.AddAsync(sectionModel);
                    await _dbContext.SaveChangesAsync();
                    response.Result = _mapper.Map<SectionViewDto>(sectionModel);
                    response.Message = "Message created successfully";
                    response.IsSuccess = true;
                    return response;
                }
                else
                {
                    response.Message = "";
                    response.Result = ModelState.Values.SelectMany(e=>e.Errors).Select(e=>e.ErrorMessage).ToList();
                    response.IsSuccess = false;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message += ex.Message;
                response.Result=null;
                response.IsSuccess=false;
                return response;
            }
        }

        /// <summary>
        /// Update Section based on supplied Id and new SectionUpdateDto object
        /// </summary>
        /// <param name="id">current section Id</param>
        /// <param name="updateDto">new SectionUpdateDto object</param>
        /// <returns>ResponseDto whcih contains updated object in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(int id, [FromBody] SectionUpdateDto updateDto)
        {
            ResponseDto response = new ();
            try
            {
                if (ModelState.IsValid)
                {
                    var sectionModel = await _dbContext.Sections.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
                    if (sectionModel == null)
                    {
                        response.Message = "Section not found";
                        response.Result = null;
                        response.IsSuccess = false;
                        return response;
                    }
                    else
                    {
                        var mappedModel = _mapper.Map<Section>(updateDto);
                        mappedModel.Id= id;
                        _dbContext.Sections.Update(mappedModel);
                        await _dbContext.SaveChangesAsync();
                        response.Result = _mapper.Map<SectionViewDto>(mappedModel);
                        response.IsSuccess=true;
                        response.Message = "Section updated successfully";
                        return response;
                    }
                }
                else
                {
                    response.Message = "";
                    response.Result = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList();
                    response.IsSuccess = false;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message += ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }
        }

        /// <summary>
        /// Delete Section based on supplied section Id 
        /// </summary>
        /// <param name="id">section Id</param>
        /// <returns>ResponseDto whcih contains deleted section in Result prop</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(int id)
        {
            ResponseDto response = new();
            try
            {
                var sectionModel = _dbContext.Sections.SingleOrDefault(x => x.Id == id);
                if (sectionModel != null)
                {
                    _dbContext.Sections.Remove(sectionModel);
                    await _dbContext.SaveChangesAsync();
                    response.Result=_mapper.Map<SectionViewDto>(sectionModel);
                    response.Message = "Section deleted successfully";
                    response.IsSuccess=false;
                    return response;
                }
                else
                {
                    response.Message = "No data";
                    response.Result = null;
                    response.IsSuccess = false;
                    return response;
                }    
            }
            catch (Exception ex)
            {
                response.Message += ex.Message;
                response.Result = null;
                response.IsSuccess = false;
                return response;
            }           
        }
    }
}

using AutoMapper;
using Azure;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Models;
using Emp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public CountriesController(AppDbContext dbContext,IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            
        }
        // GET: api/<CountriesController>
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> Get()
        {
            var _response = new ResponseDto();
            var counries = await _dbContext.Countries.Where(a => a.IsDeleted == false).ToListAsync();
            _response.Result = _mapper.Map<List<CountryCreateDto>>(counries);
            _response.IsSuccess = true;
            _response.Message = "Countries retrieved successfully.";
            return Ok(_response);
        }

        // GET api/<CountriesController>/5
        [HttpGet("{id}")]
        public async Task<ResponseDto> Get(int id)
        {
            var _response = new ResponseDto();
            Country country = await _dbContext.Countries.SingleOrDefaultAsync(x => x.Id == id);
            if (country == null)
            {
                _response.Result = null;
                _response.IsSuccess = false;
                _response.Message = "Country not found";
                return _response;
            }
            else
            {
                _response.IsSuccess=true;
                _response.Result= _mapper.Map<CountryCreateDto>(country);
                _response.Message = "";
                return _response;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ResponseDto> Post([FromBody] CountryCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid model data.",
                    Result = null
                };
            }

            var mappedModel = _mapper.Map<Country>(createDto);
            _dbContext.Countries.Add(mappedModel);
            await _dbContext.SaveChangesAsync();

            return new ResponseDto
            {
                Result = new { mappedModel.Id, mappedModel.Name }, // You can customize this
                Message = "Country created successfully.",
                IsSuccess = true
            };
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ResponseDto> Put(int id, [FromBody] CountryCreateDto countryUpdateDto)
        {
            var response = new ResponseDto();

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Message = "Invalid data.";
                return response;
            }

            var existingCountry = await _dbContext.Countries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (existingCountry == null)
            {
                response.IsSuccess = false;
                response.Message = $"Country with ID {id} not found.";
                return response;
            }

            countryUpdateDto.Id = id; // ensure the ID matches route
            Country countryToUpdate = _mapper.Map<Country>(countryUpdateDto);
            _dbContext.Countries.Update(countryToUpdate);
            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Country updated successfully.";
            response.Result = new { countryUpdateDto.Id, countryUpdateDto.Name }; // optional

            return response;
        }


        // DELETE api/<CountriesController>/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ResponseDto> Delete(int id)
        {
            ResponseDto response = new ResponseDto();

            var country = await _dbContext.Countries.FindAsync(id);
            if (country == null)
            {
                response.IsSuccess = false;
                response.Message = $"Country with ID {id} not found.";
                return response;
            }

            _dbContext.Countries.Remove(country);
            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Country deleted successfully.";
            response.Result = true;

            return response;
        }


    }
}

using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class CountryService : ICountryService
    {
        private readonly IBaseService _baseService;
        public CountryService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateCountryAsync(CountryDto countryDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = SD.CountriesAPIUrl,
                Data = countryDto
            });
        }

        public async Task<ResponseDto> UpdateCountryAsync(CountryDto countryDto)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Put,
                Url = SD.CountriesAPIUrl + "/" + countryDto.Id,
                Data = countryDto
            });
        }

        public async Task<ResponseDto> DeleteCountryAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Delete,
                Url = SD.CountriesAPIUrl + "/" + id,
            });
        }

        public async Task<ResponseDto> GetCountriesAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.CountriesAPIUrl
            });
        }

        public async Task<ResponseDto> GetCountryAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = SD.CountriesAPIUrl+ "/" + id
            });
        }
    }
}

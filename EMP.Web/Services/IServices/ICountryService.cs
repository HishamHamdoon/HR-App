using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface ICountryService
    {
        Task<ResponseDto> GetCountriesAsync();
        Task<ResponseDto> GetCountryAsync(int id);
        Task<ResponseDto> CreateCountryAsync(CountryDto countryDto);
        Task<ResponseDto> UpdateCountryAsync(CountryDto countryDto);
        Task<ResponseDto> DeleteCountryAsync(int id);

    }
}

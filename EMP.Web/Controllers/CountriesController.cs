//using Emp.Web.Dtos.Models;
using Emp.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class CountriesController : Controller
    {
        private readonly ICountryService _countryService;
        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }
        public async Task<IActionResult> Index()
        {
            ResponseDto response = await _countryService.GetCountriesAsync();
            var countriesList = new List<CountryDto>();
            if (response.IsSuccess && response.Result is not null)
            {
                countriesList = JsonConvert.DeserializeObject<List<CountryDto>>(response.Result.ToString());

                return View(countriesList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            ResponseDto response = await _countryService.GetCountryAsync(id);
            var countriesList = new CountryDto();
            if (response.IsSuccess && response.Result is not null)
            {
                countriesList = JsonConvert.DeserializeObject<CountryDto>(response.Result.ToString());

                return View(countriesList);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CountryDto countryDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _countryService.CreateCountryAsync(countryDto);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                return View(countryDto);

            }
            else
            {
                return View(countryDto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _countryService.GetCountryAsync(id);
            if (response?.IsSuccess == true && response.Result is not null)
            {
                var country = JsonConvert.DeserializeObject<CountryDto>(response.Result.ToString());
                return View(country);
            }
            TempData["error"] = "Country not found.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CountryDto countryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(countryDto);
            }
            var response = await _countryService.UpdateCountryAsync(countryDto);
            if (response?.IsSuccess == true)
            {
                TempData["success"] = response.Message ?? "Country updated successfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = response?.Message ?? "Failed to update country.";
            return View(countryDto);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {

            ResponseDto response = await _countryService.GetCountryAsync(id);
            var country = new CountryDto();
            if (response.IsSuccess && response.Result is not null)
            {
                country = JsonConvert.DeserializeObject<CountryDto>(response.Result.ToString());

                return View(country);
            }
            else
            {
                TempData["error"] = "Something went wrong!";
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (id > 0)
            {
                var response = await _countryService.DeleteCountryAsync(id);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Something went wrong";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (id > 0)
            {
                var response = await _countryService.DeleteCountryAsync(id);
                if (response.IsSuccess)
                {
                    TempData["Success"] = response.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Something went wrong";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");

        }

        }
}

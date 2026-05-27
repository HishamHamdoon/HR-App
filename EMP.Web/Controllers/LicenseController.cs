using Emp.Api.Controllers;
using EMP.Web.Middleware;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EMP.Web.Controllers
{
    [Authorize]
    public class LicenseController : BaseController
    {
        private readonly ILicenseService _licenseService;
        private readonly IMemoryCache _cache;

        public LicenseController(ILicenseService licenseService, IMemoryCache cache)
        {
            _licenseService = licenseService;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Status = await LoadStatusAsync();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string key)
        {
            var response = await _licenseService.ActivateAsync(key ?? string.Empty);
            if (response?.IsSuccess == true)
            {
                _cache.Remove(LicenseEnforcementMiddleware.CacheKey); // reflect new status immediately
            }
            TempData[response?.IsSuccess == true ? "success" : "error"] =
                response?.Message ?? (response?.IsSuccess == true ? "License activated." : "Activation failed.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string type = "Yearly", int months = 12)
        {
            var response = await _licenseService.GenerateAsync(type, months);
            if (response?.IsSuccess == true && response.Result is not null)
            {
                TempData["generatedKey"] = Convert.ToString(response.Result);
                TempData["success"] = response.Message;
            }
            else
            {
                TempData["error"] = response?.Message ?? "Could not generate a key.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<LicenseStatusDto> LoadStatusAsync()
        {
            try
            {
                var response = await _licenseService.GetStatusAsync();
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    return JsonConvert.DeserializeObject<LicenseStatusDto>(Convert.ToString(response.Result))
                           ?? new LicenseStatusDto { Type = "Unknown" };
                }
            }
            catch { /* fall through */ }
            return new LicenseStatusDto { Type = "Unknown" };
        }
    }
}

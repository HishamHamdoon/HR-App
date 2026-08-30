using Emp.Api.Controllers;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using EMP.Web.ViewComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EMP.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly IMemoryCache _cache;

        public SettingsController(ISettingsService settingsService, IMemoryCache cache)
        {
            _settingsService = settingsService;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CompanySettingsDto model, IFormFile? logo, bool removeLogo = false)
        {
            if (removeLogo)
            {
                model.LogoBase64 = ""; // empty string clears the logo on the API
            }
            else if (logo is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await logo.CopyToAsync(ms);
                model.LogoBase64 = Convert.ToBase64String(ms.ToArray());
            }
            else
            {
                model.LogoBase64 = null; // leave existing logo unchanged
            }

            var response = await _settingsService.UpdateAsync(model);

            // The session-timeout dialog reads a cached copy of this value; drop it so a
            // new period takes effect on the next page load instead of up to 5 minutes later.
            _cache.Remove(SessionTimeoutViewComponent.CacheKey);

            TempData[response?.IsSuccess == true ? "success" : "error"] =
                response?.Message ?? (response?.IsSuccess == true ? "Settings saved." : "Could not save settings.");

            return RedirectToAction(nameof(Index));
        }
    }
}

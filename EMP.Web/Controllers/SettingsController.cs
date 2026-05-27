using Emp.Api.Controllers;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMP.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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
            TempData[response?.IsSuccess == true ? "success" : "error"] =
                response?.Message ?? (response?.IsSuccess == true ? "Settings saved." : "Could not save settings.");

            return RedirectToAction(nameof(Index));
        }
    }
}

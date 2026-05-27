using EMP.Web.Middleware;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EMP.Web.ViewComponents
{
    // Shows a dismissible warning when the active license is close to expiring.
    public class LicenseBannerViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;
        private readonly ILicenseService _licenseService;

        public LicenseBannerViewComponent(IMemoryCache cache, ILicenseService licenseService)
        {
            _cache = cache;
            _licenseService = licenseService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            LicenseStatusDto? status = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                if (!_cache.TryGetValue(LicenseEnforcementMiddleware.CacheKey, out status))
                {
                    try
                    {
                        var response = await _licenseService.GetStatusAsync();
                        if (response?.IsSuccess == true && response.Result is not null)
                        {
                            status = JsonConvert.DeserializeObject<LicenseStatusDto>(Convert.ToString(response.Result));
                            if (status is not null)
                            {
                                _cache.Set(LicenseEnforcementMiddleware.CacheKey, status, TimeSpan.FromSeconds(60));
                            }
                        }
                    }
                    catch { /* no banner if status unavailable */ }
                }
            }

            return View(status);
        }
    }
}

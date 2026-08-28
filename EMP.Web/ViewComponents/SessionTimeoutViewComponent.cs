using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EMP.Web.ViewComponents
{
    // Renders the idle-session warning dialog. The idle period is an organisation
    // setting (CompanySettings.SessionTimeoutMinutes); 0 means the dialog is not
    // rendered at all. The value is cached briefly so every page view doesn't call
    // the API — SettingsController drops the entry when an admin saves a new value.
    public class SessionTimeoutViewComponent : ViewComponent
    {
        public const string CacheKey = "settings:session-timeout-minutes";

        private readonly IMemoryCache _cache;
        private readonly ISettingsService _settingsService;

        public SessionTimeoutViewComponent(IMemoryCache cache, ISettingsService settingsService)
        {
            _cache = cache;
            _settingsService = settingsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return View(0);
            }

            if (!_cache.TryGetValue(CacheKey, out int minutes))
            {
                // GetSettingsAsync falls back to the DTO defaults if the API is unreachable,
                // so an outage leaves the check on rather than silently disabling it.
                var settings = await _settingsService.GetSettingsAsync();
                minutes = Math.Clamp(settings.SessionTimeoutMinutes, 0, 1440);
                _cache.Set(CacheKey, minutes, TimeSpan.FromMinutes(5));
            }

            return View(minutes);
        }
    }
}

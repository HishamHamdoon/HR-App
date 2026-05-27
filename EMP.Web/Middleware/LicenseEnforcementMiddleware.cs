using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EMP.Web.Middleware
{
    /// <summary>
    /// Locks the application when no valid license is active. The license page, account
    /// pages and static assets stay reachable so an admin can re-activate. Fails open if
    /// the license status can't be determined (e.g. API blip) to avoid lockouts on outages.
    /// </summary>
    public class LicenseEnforcementMiddleware
    {
        public const string CacheKey = "license-status";
        private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(60);

        private readonly RequestDelegate _next;

        public LicenseEnforcementMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache cache, ILicenseService licenseService)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (context.User?.Identity?.IsAuthenticated != true || IsExempt(path))
            {
                await _next(context);
                return;
            }

            var status = await GetStatusAsync(cache, licenseService);

            // Only block when we positively know the license is invalid.
            if (status is { IsValid: false })
            {
                context.Response.Redirect("/License");
                return;
            }

            // Force a first-login password change before anything else (Account pages are exempt
            // above, so the change-password form itself stays reachable).
            if (context.User.HasClaim("MustChangePassword", "true"))
            {
                context.Response.Redirect("/Account/ChangePassword");
                return;
            }

            await _next(context);
        }

        private static async Task<LicenseStatusDto?> GetStatusAsync(IMemoryCache cache, ILicenseService licenseService)
        {
            if (cache.TryGetValue(CacheKey, out LicenseStatusDto? cached))
            {
                return cached;
            }

            try
            {
                var response = await licenseService.GetStatusAsync();
                if (response?.IsSuccess == true && response.Result is not null)
                {
                    var status = JsonConvert.DeserializeObject<LicenseStatusDto>(Convert.ToString(response.Result));
                    if (status is not null)
                    {
                        cache.Set(CacheKey, status, CacheFor);
                        return status;
                    }
                }
            }
            catch
            {
                // Fail open.
            }
            return null;
        }

        private static bool IsExempt(string path)
        {
            path = path.ToLowerInvariant();

            string[] prefixes =
            {
                "/license", "/account", "/lib", "/vuexy", "/css", "/js",
                "/toastr.js", "/uploads", "/favicon", "/home/error"
            };
            foreach (var p in prefixes)
            {
                if (path.StartsWith(p)) return true;
            }

            // Any request for a file with an extension (static asset) is allowed through.
            var lastSlash = path.LastIndexOf('/');
            var lastSegment = lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
            return lastSegment.Contains('.');
        }
    }
}

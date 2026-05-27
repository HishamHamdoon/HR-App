using Emp.Api.Dtos.License;

namespace Emp.Api.Services.IServices
{
    public interface ILicenseService
    {
        /// <summary>Creates the 30-day trial on first run if no license exists.</summary>
        Task EnsureTrialAsync();

        /// <summary>Current license status (uses the license with the furthest expiry).</summary>
        Task<LicenseStatusDto> GetStatusAsync();

        /// <summary>Validates and activates a signed key. Returns (success, message, status).</summary>
        Task<(bool ok, string message, LicenseStatusDto status)> ActivateAsync(string key);

        /// <summary>Vendor-side helper: produce a signed key valid until the given expiry.</summary>
        string GenerateKey(string type, DateTime expiresAt);
    }
}

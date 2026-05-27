using Emp.Api.Data;
using Emp.Api.Dtos.License;
using Emp.Api.Models;
using Emp.Api.Services.IServices;
using Emp.Models.Licensing;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Emp.Api.Services
{
    /// <summary>
    /// Offline licensing. Keys are self-contained and tamper-proof: the payload
    /// ("Type|yyyy-MM-dd") is signed with an HMAC over a server-side secret, so a key
    /// can be verified without contacting a license server and cannot be forged or
    /// back-dated without the secret.
    /// </summary>
    public class LicenseService : ILicenseService
    {
        public const string Trial = "Trial";
        public const string Yearly = "Yearly";

        private readonly AppDbContext _dbContext;
        private readonly byte[] _secret;

        public LicenseService(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            var secret = configuration["Licensing:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                // Fallback keeps the app runnable; override via config in real deployments.
                secret = "CHANGE-ME-LICENSING-SECRET-0123456789ABCDEF";
            }
            _secret = Encoding.UTF8.GetBytes(secret);
        }

        public async Task EnsureTrialAsync()
        {
            if (await _dbContext.Licenses.AnyAsync())
            {
                return;
            }
            _dbContext.Licenses.Add(new License
            {
                Type = Trial,
                Key = null,
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.Date.AddDays(30),
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<LicenseStatusDto> GetStatusAsync()
        {
            // The effective license is the one that grants access the longest.
            var license = await _dbContext.Licenses
                .OrderByDescending(l => l.ExpiresAt)
                .FirstOrDefaultAsync();

            if (license is null)
            {
                return new LicenseStatusDto { Type = "None", IsValid = false, DaysRemaining = 0, ExpiresAt = DateTime.MinValue };
            }
            return BuildStatus(license.Type, license.ExpiresAt);
        }

        public async Task<(bool ok, string message, LicenseStatusDto status)> ActivateAsync(string key)
        {
            var current = await GetStatusAsync();

            if (string.IsNullOrWhiteSpace(key) || !LicenseKeyCodec.TryVerify(key, _secret, out var type, out var expiresAt))
            {
                return (false, "Invalid or tampered license key.", current);
            }
            if (expiresAt.Date < DateTime.Now.Date)
            {
                return (false, "This license key has already expired.", current);
            }

            _dbContext.Licenses.Add(new License
            {
                Type = type,
                Key = key,
                IssuedAt = DateTime.Now,
                ExpiresAt = expiresAt,
            });
            await _dbContext.SaveChangesAsync();

            return (true, $"{type} license activated until {expiresAt:dd MMM yyyy}.", BuildStatus(type, expiresAt));
        }

        public string GenerateKey(string type, DateTime expiresAt) =>
            LicenseKeyCodec.Generate(type, expiresAt, _secret);

        private LicenseStatusDto BuildStatus(string type, DateTime expiresAt)
        {
            var days = (expiresAt.Date - DateTime.Now.Date).Days;
            return new LicenseStatusDto
            {
                Type = type,
                ExpiresAt = expiresAt,
                IsValid = expiresAt.Date >= DateTime.Now.Date,
                DaysRemaining = days < 0 ? 0 : days,
            };
        }
    }
}

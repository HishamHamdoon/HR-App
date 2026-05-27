using Emp.Api.Dtos;
using Emp.Api.Dtos.License;
using Emp.Api.Services;
using Emp.Api.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IWebHostEnvironment _env;

        public LicenseController(ILicenseService licenseService, IWebHostEnvironment env)
        {
            _licenseService = licenseService;
            _env = env;
        }

        [HttpGet("status")]
        public async Task<ResponseDto> Status()
        {
            return new ResponseDto { IsSuccess = true, Result = await _licenseService.GetStatusAsync() };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("activate")]
        public async Task<ResponseDto> Activate([FromBody] ActivateLicenseDto dto)
        {
            var (ok, message, status) = await _licenseService.ActivateAsync(dto.Key);
            return new ResponseDto { IsSuccess = ok, Message = message, Result = status };
        }

        /// <summary>
        /// Vendor/testing helper to mint a key. Development-only so production builds
        /// can't self-issue licenses.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("generate")]
        public ResponseDto Generate(string type = "Yearly", int months = 12)
        {
            if (!_env.IsDevelopment())
            {
                return new ResponseDto { IsSuccess = false, Message = "Key generation is disabled in this environment." };
            }
            var normalizedType = string.Equals(type, "Trial", StringComparison.OrdinalIgnoreCase)
                ? LicenseService.Trial : LicenseService.Yearly;
            var expiry = DateTime.Now.Date.AddMonths(months <= 0 ? 12 : months);
            var key = _licenseService.GenerateKey(normalizedType, expiry);
            return new ResponseDto { IsSuccess = true, Result = key, Message = $"{normalizedType} key valid until {expiry:dd MMM yyyy}." };
        }
    }
}

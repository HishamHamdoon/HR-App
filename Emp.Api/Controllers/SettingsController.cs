using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Settings;
using Emp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public SettingsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            var settings = await GetOrCreateAsync();
            return new ResponseDto { IsSuccess = true, Result = ToDto(settings) };
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ResponseDto> Update([FromBody] CompanySettingsDto dto)
        {
            var settings = await GetOrCreateAsync();
            settings.CompanyName = string.IsNullOrWhiteSpace(dto.CompanyName) ? settings.CompanyName : dto.CompanyName;
            settings.Address = dto.Address;
            settings.Phone = dto.Phone;
            settings.Email = dto.Email;
            settings.RequirePasswordChangeOnFirstLogin = dto.RequirePasswordChangeOnFirstLogin;
            settings.DefaultCalendar = string.IsNullOrWhiteSpace(dto.DefaultCalendar) ? "Gregorian" : dto.DefaultCalendar;
            if (dto.LogoBase64 is not null)
            {
                // Empty string clears the logo; null leaves it unchanged.
                settings.LogoBase64 = dto.LogoBase64.Length == 0 ? null : dto.LogoBase64;
            }
            await _dbContext.SaveChangesAsync();

            return new ResponseDto { IsSuccess = true, Message = "Settings saved.", Result = ToDto(settings) };
        }

        private async Task<CompanySettings> GetOrCreateAsync()
        {
            var settings = await _dbContext.CompanySettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (settings is null)
            {
                settings = new CompanySettings { CompanyName = "Your Company" };
                _dbContext.CompanySettings.Add(settings);
                await _dbContext.SaveChangesAsync();
            }
            return settings;
        }

        private static CompanySettingsDto ToDto(CompanySettings s) => new()
        {
            CompanyName = s.CompanyName,
            Address = s.Address,
            Phone = s.Phone,
            Email = s.Email,
            LogoBase64 = s.LogoBase64,
            RequirePasswordChangeOnFirstLogin = s.RequirePasswordChangeOnFirstLogin,
            DefaultCalendar = s.DefaultCalendar,
        };
    }
}

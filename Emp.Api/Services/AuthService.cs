using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Auth;
using Emp.Api.Models;
using Emp.Api.Services.IServices;
using Emp.Models.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Emp.Api.Services
{

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ResponseDto _response;
        // The error is:
        // CS0311: The type 'Emp.Models.Models.ApplicationUser' cannot be used as type parameter 'TUser' in the generic type or method 'UserManager<TUser>'. There is no implicit reference conversion from 'Emp.Models.Models.ApplicationUser' to 'Microsoft.AspNet.Identity.IUser<string>'.

        // This means that your ApplicationUser class does not implement the required interface IUser<string> from Microsoft.AspNet.Identity.
        // To fix this, ensure that ApplicationUser inherits from IdentityUser (which implements IUser<string>), and that you are referencing the correct IdentityUser base class from Microsoft.AspNet.Identity, not from a different namespace or version.
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(AppDbContext context, IJwtTokenGenerator jwtTokenGenerator, IConfiguration config, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _response = new();
        }

        //public async Task<string> Register(string username, string password)
        //{
        //    if (_context.Users.Any(u => u.Username == username))
        //        throw new Exception("User already exists");

        //    CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        //    var user = new User
        //    {
        //        Username = username,
        //        PasswordHash = hash,
        //        PasswordSalt = salt
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return CreateToken(user);
        //}

        //public async Task<string?> Login(string username, string password)

        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        //    if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        //        return null;

        //    return CreateToken(user);
        //}

        //private string CreateToken(User user)
        //{
        //    var claims = new List<Claim>
        //{
        //    new(ClaimTypes.Name, user.Username)
        //};

        //    var key = new SymmetricSecurityKey(
        //        System.Text.Encoding.UTF8.GetBytes(_config["AppSettings:Token"]!));

        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        //    var token = new JwtSecurityToken(
        //        claims: claims,
        //        expires: DateTime.Now.AddHours(1),
        //        signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        //{
        //    using var hmac = new HMACSHA512();
        //    salt = hmac.Key;
        //    hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //}

        //private bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        //{
        //    using var hmac = new HMACSHA512(salt);
        //    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //    return computedHash.SequenceEqual(hash);
        //}

        public async Task<ResponseDto> Register(RegisterDto registerDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Create Employee
                var employee = new Employee
                {
                    Name = registerDto.Name,
                    Address = registerDto.Address,
                    BirthDate = registerDto.BirthDate,
                    HireDate = DateOnly.FromDateTime(DateTime.Now),
                    CountryId = registerDto.CountryId,
                    DepartmentId = registerDto.DepartmentId,
                    JobTitleId = registerDto.JobTitleId,
                    IsActive = true,
                    Email = registerDto.Email,
                    Phone = registerDto.Phone
                };

                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();

                // Honour the org policy: force a password change at first login when enabled.
                var requireChange = await _context.CompanySettings
                    .OrderBy(s => s.Id)
                    .Select(s => (bool?)s.RequirePasswordChangeOnFirstLogin)
                    .FirstOrDefaultAsync() ?? true;

                // Step 2: Create Identity User
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    EmployeeId = employee.Id,
                    MustChangePassword = requireChange
                };

                // Default password when none supplied.
                var password = string.IsNullOrWhiteSpace(registerDto.Password)
                    ? DbInitializer.DefaultUserPassword
                    : registerDto.Password;

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    _response.Message = result.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
                    _response.IsSuccess = false;
                    _response.Result = null;
                    return _response;
                }

                // Assign default role only after the user is successfully created.
                await AssignRole(user.Email, DbInitializer.EmployeeRole);

                await transaction.CommitAsync();

                _response.IsSuccess = true;
                _response.Result = true;
                _response.Message = "Employee created successfully";
                return _response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _response.Message = $"Registration failed: {ex.Message}";
                _response.IsSuccess = false;
                _response.Result = null;
                return _response;
            }
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _context.ApplicationUsers
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(a => a.UserName == loginRequestDto.Username);

            if (user is null)
            {
                return new LoginResponseDto();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new LoginResponseDto { Token = "", User = null };
            }

            var valid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (!valid)
            {
                await _userManager.AccessFailedAsync(user);
                return new LoginResponseDto { Token = "", User = null };
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var (token, refreshToken) = await IssueTokensAsync(user);

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Email = user.Email,
                    Name = user.UserName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber
                }
            };
        }

        /// <summary>
        /// Exchanges a valid refresh token for a new access token, rotating the refresh
        /// token (the presented one is revoked and a new one issued). Returns a response
        /// with an empty Token when the refresh token is missing, expired, or revoked.
        /// </summary>
        public async Task<LoginResponseDto?> Refresh(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == hash);

            if (stored is null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            {
                return new LoginResponseDto { Token = "", User = null };
            }

            var user = await _context.ApplicationUsers
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == stored.UserId);
            if (user is null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new LoginResponseDto { Token = "", User = null };
            }

            // Rotate: revoke the presented token, then issue a fresh pair.
            stored.RevokedAt = DateTime.UtcNow;
            var (token, newRefresh) = await IssueTokensAsync(user);

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = newRefresh,
                User = new UserDto
                {
                    Email = user.Email,
                    Name = user.UserName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber
                }
            };
        }

        /// <summary>Revokes a refresh token (sign-out). No-op if it is unknown or already revoked.</summary>
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null);
            if (stored != null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Builds the claim inputs, generates an access token, and issues+persists a new
        /// refresh token. Shared by login and refresh so both produce identical claims.
        /// </summary>
        private async Task<(string accessToken, string refreshToken)> IssueTokensAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // A user is a manager if they own at least one department.
            var isManager = user.Employee != null
                && await _context.Departments.AnyAsync(d => d.ManagerId == user.Employee.Id);

            // Resolve UI preferences (calendar falls back to the org default).
            var theme = string.IsNullOrWhiteSpace(user.PreferredTheme) ? "light" : user.PreferredTheme;
            var orgCalendar = await _context.CompanySettings
                .OrderBy(s => s.Id).Select(s => s.DefaultCalendar).FirstOrDefaultAsync() ?? "Gregorian";
            var calendar = string.IsNullOrWhiteSpace(user.PreferredCalendar) ? orgCalendar : user.PreferredCalendar;
            var lang = string.IsNullOrWhiteSpace(user.PreferredLanguage) ? "en" : user.PreferredLanguage;

            var accessToken = await _jwtTokenGenerator.GenerateToken(
                user, roles, isManager, user.MustChangePassword, theme, calendar, lang);

            // Opaque 256-bit token; only its hash is stored.
            var raw = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(raw),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });
            await _context.SaveChangesAsync();

            return (accessToken, raw);
        }

        private static string HashToken(string raw)
            => Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)));

        private static string Base64UrlEncode(byte[] bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public async Task<ResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var response = new ResponseDto();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.IsSuccess = false;
                response.Message = "User not found.";
                return response;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                if (user.MustChangePassword)
                {
                    user.MustChangePassword = false;
                    await _userManager.UpdateAsync(user);
                }
                response.IsSuccess = true;
                response.Result = true;
                response.Message = "Password changed successfully.";
            }
            else
            {
                response.IsSuccess = false;
                response.Message = string.Join("; ", result.Errors.Select(e => e.Description));
            }
            return response;
        }

        public async Task<ResponseDto> UpdatePreferencesAsync(string userId, string? theme, string? calendar, string? language)
        {
            var response = new ResponseDto();
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.IsSuccess = false;
                response.Message = "User not found.";
                return response;
            }

            if (theme is not null)
            {
                user.PreferredTheme = theme.Equals("dark", StringComparison.OrdinalIgnoreCase) ? "dark" : "light";
            }
            if (calendar is not null)
            {
                user.PreferredCalendar = calendar.Equals("Hijri", StringComparison.OrdinalIgnoreCase) ? "Hijri" : "Gregorian";
            }
            if (language is not null)
            {
                user.PreferredLanguage = language.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
            }
            await _userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.Message = "Preferences saved.";
            response.Result = new { Theme = user.PreferredTheme, Calendar = user.PreferredCalendar, Language = user.PreferredLanguage };
            return response;
        }

        public async Task<ResponseDto?> AssignRole(string email, string roleName)
        {
            var response = new ResponseDto();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response.Result = false;
                response.Message = "User not found";
                response.IsSuccess = false;
                return response;
            }

            // Create role if it does not exist
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    response.Result = false;
                    response.Message = "Failed to create role";
                    response.IsSuccess = false;
                    return response;
                }
            }

            // Assign role to user
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                response.Result = true;
                response.Message = "User already in role";
                response.IsSuccess = true;
                return response;
            }

            var assignResult = await _userManager.AddToRoleAsync(user, roleName);
            if (assignResult.Succeeded)
            {
                response.Result = true;
                response.Message = "Role assigned successfully";
                response.IsSuccess = true;
            }
            else
            {
                response.Result = false;
                response.Message = string.Join("; ", assignResult.Errors.Select(e => e.Description));
                response.IsSuccess = false;
            }

            return response;
        }
    }
}
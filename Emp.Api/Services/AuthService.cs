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

                // Step 2: Create Identity User
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    EmployeeId = employee.Id
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    _response.Message = result.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
                    _response.IsSuccess = false;
                    _response.Result = null;
                }
                else
                {
                    _response.IsSuccess = true;
                    _response.Result = true;
                    _response.Message = "Employee created successfully";
                }

                    // role: assign to default role
                    await AssignRole(user.Email, "Employee");

                await transaction.CommitAsync();
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
            var user = await _context.ApplicationUsers.Include(e=>e.Employee).FirstOrDefaultAsync(a => a.UserName == loginRequestDto.Username);
            if (user is not null)
            {
                bool valid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
                var roles = await _userManager.GetRolesAsync(user);
                string token = await _jwtTokenGenerator.GenerateToken(user,roles);
                if (valid)
                {
                    //generate token
                    //token = await _jwtTokenGenerator.GenerateToken(user);

                }
                else
                {
                    return new LoginResponseDto()
                    {
                        Token = "",
                        User = null
                    };
                }
                UserDto userDto = new()
                {
                    Email = user.Email,
                    Name = user.UserName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber
                };
                LoginResponseDto loginResponse = new LoginResponseDto()
                {
                    Token = token,
                    User = userDto
                };
                return loginResponse;
            }
            else
            {
                return new LoginResponseDto();
            }

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
using Emp.Api.Data;
using Emp.Api.Models;
using EMP.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EMP.Services.Implementations
{

    public class AuthService : IAuthService
    {
        private readonly AppContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> Register(string username, string password)
        {
            if (_context.Users.Any(u => u.Username == username))
                throw new Exception("User already exists");

            CreatePasswordHash(password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreateToken(user);
        }

        public async Task<string?> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return CreateToken(user);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username)
        };

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["AppSettings:Token"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
    }

}
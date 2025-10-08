using Emp.Api.Dtos;
using Emp.Api.Services.IServices;
using Emp.Models.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Emp.Api.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;
        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public async Task<string> GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
            if (applicationUser.Employee != null)
            {
                new Claim("EmployeeId", applicationUser.Employee.Id.ToString());
            }
            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new Claim(JwtRegisteredClaimNames.Name, applicationUser.UserName),
                new Claim("EmployeeId", applicationUser.Employee.Id.ToString())
            };
            claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var tokendescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claimList),
                //Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials= new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokendescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

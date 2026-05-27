using System.IdentityModel.Tokens.Jwt;
using Emp.Api.Dtos;
using Emp.Api.Models;
using Emp.Api.Services;
using Emp.Models.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace Emp.Tests
{
    public class JwtTokenGeneratorTests
    {
        private static JwtTokenGenerator CreateGenerator() =>
            new JwtTokenGenerator(Options.Create(new JwtOptions
            {
                Secret = "unit-test-secret-key-at-least-32-characters-long",
                Issuer = "Emp.Api",
                Audience = "Emp.Web"
            }));

        private static ApplicationUser SampleUser() => new ApplicationUser
        {
            Id = "user-1",
            UserName = "admin",
            Email = "admin@admin.com",
            Employee = new Employee { Id = 42, Name = "Super Admin" }
        };

        [Fact]
        public async Task GenerateToken_Embeds_Role_Claims()
        {
            var token = await CreateGenerator().GenerateToken(SampleUser(), new[] { "Admin", "Employee" });

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var roles = jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

            Assert.Contains("Admin", roles);
            Assert.Contains("Employee", roles);
        }

        [Fact]
        public async Task GenerateToken_Sets_Issuer_Audience_And_Name()
        {
            var token = await CreateGenerator().GenerateToken(SampleUser(), Array.Empty<string>());

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal("Emp.Api", jwt.Issuer);
            Assert.Contains("Emp.Web", jwt.Audiences);
            Assert.Contains(jwt.Claims, c => c.Type == "EmployeeId" && c.Value == "42");
            Assert.Contains(jwt.Claims, c => c.Value == "admin");
        }

        [Fact]
        public async Task GenerateToken_Produces_NonEmpty_Token()
        {
            var token = await CreateGenerator().GenerateToken(SampleUser(), Array.Empty<string>());
            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}

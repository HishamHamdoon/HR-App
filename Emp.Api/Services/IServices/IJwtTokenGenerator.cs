using Emp.Models.Models;

namespace Emp.Api.Services.IServices
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateToken(ApplicationUser applicationUser,IEnumerable<string> roles);
    }
}

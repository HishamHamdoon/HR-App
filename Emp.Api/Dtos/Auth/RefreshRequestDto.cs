using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Dtos.Auth
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

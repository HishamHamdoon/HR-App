using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Dtos.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdatePreferencesDto
    {
        public string? Theme { get; set; }
        public string? Calendar { get; set; }
        public string? Language { get; set; }
    }
}

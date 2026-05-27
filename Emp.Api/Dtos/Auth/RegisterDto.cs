using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Dtos.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public DateOnly? LeavingDate { get; set; }
        public int CountryId { get; set; }
        public int DepartmentId { get; set; }
        public int JobTitleId { get; set; }
        

    }
    public class CreateRoleDto
    {
        [Required]
        public string? Role { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}

using Emp.Api.Models;

namespace Emp.Web.Dtos.Auth
{
    public class RegisterDto
    {
        public Employee Employee { get; set; } = new Employee();

        public List<Department> Departments { get; set; } = new();
        public List<Section> Sections { get; set; } = new();
        public List<Emp.Api.Models.JobTitle> JobTitles { get; set; } = new();
        public List<Country> Countries { get; set; } = new();
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
        public int ManagerId { get; set; }
    }
    //public class CreateRoleDto
    //{
    //    [Required]
    //    public string? Role { get; set; }
    //    [Required]
    //    public string Email { get; set; } = string.Empty;
    //}
}

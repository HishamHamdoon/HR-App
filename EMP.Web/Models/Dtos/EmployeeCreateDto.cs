using System.ComponentModel.DataAnnotations;

namespace Emp.Web.Models.Dtos
{
    public class EmployeeCreateDto
    {
        public int Id { get; set; }
        [Required,MaxLength(100)]
        public string Name { get; set; }
        [Required,EmailAddress]
        public string Email { get; set; }
        [Required,MaxLength(20)]
        public string Phone { get; set; }
        [Required,MaxLength(150)]
        public string? Address { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        //[MinimumYearValidator]

        public DateOnly BirthDate { get; set; }
        [Required]
        public DateOnly HireDate { get; set; }= DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? LeavingDate { get; set; }

        //public int Depid { get; set; }
        public int DepartmentId { get; set; }
        public int JobTitleId { get; set; }
        public int CountryId { get; set; } 
        public int? ManagerId { get; set; } 

    }
}


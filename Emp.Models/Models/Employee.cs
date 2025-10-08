using Emp.Api.CustomeValidations;
using Emp.Models;
using Emp.Models.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Models
{
    public class Employee
    {
        //add contoury - qualifications - maritual status - relegion - gender - section
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public DateOnly LeavingDate { get; set; }

        public int  DepartmentId{ get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
        public int? SectionId { get; set; }
        [ForeignKey("SectionId")]
        public Section? Section{ get; set; }
        public int JobTitleId { get; set; }
        [ForeignKey("JobTitleId")]
        public JobTitle? JobTitle { get; set; }
        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public Country? Country { get; set; }
        // Self-referencing relationship for Employee's Manager
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public ApplicationUser? User { get; set; }
        public Salary Salary{ get; set; }

    }
}

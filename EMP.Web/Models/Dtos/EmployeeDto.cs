using Emp.Api.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMP.Web.Models.Dtos
{
    public class 
        
        EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public DateOnly LeavingDate { get; set; }
        public Department? Department { get; set; }
        public Section? Section { get; set; }
        public JobTitle? JobTitle { get; set; }
        public Country? Country { get; set; }

    }
}

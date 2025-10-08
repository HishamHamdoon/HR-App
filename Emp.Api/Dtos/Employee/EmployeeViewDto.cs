using Emp.Api.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Employee
{
    public class EmployeeViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool isActive { get; set; }
        //public DateTime BirthDate { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitleTitle { get; set; }
        public string CountryName { get; set; }
        public string Manager { get; set; }
        public int DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int JobTitleId { get; set; }
        public int CountryId { get; set; }
        public int? ManagerId { get; set; }
    }
}

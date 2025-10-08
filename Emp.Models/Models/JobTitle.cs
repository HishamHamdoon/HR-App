using Emp.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Models
{
    public class JobTitle:BaseModel
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal MainSalary { get; set; }
        //number of jobs per department
        public int? Slots { get; set; }
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department{ get; set; }
        public List<Employee> Employees { get; set; }
    }
}

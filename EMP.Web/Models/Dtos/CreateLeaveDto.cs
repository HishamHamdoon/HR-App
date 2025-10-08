using Emp.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Web.Dtos
{
    public class CreateLeaveDto
    {
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly? EndDate { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int LeavesTypeId { get; set; }
        public int ManagerId { get; set; }
        public string? Note { get; set; }
        public string? FilePath { get; set; }
        public IFormFile? Attachment { get; set; }
        public List<Employee>? Employees { get; set; }
        public List<LeavesType>? LeavesTypes { get; set; }
    }
    public class CreateLeaveVM
    {
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly? EndDate { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int LeavesTypeId { get; set; }
        public string? Note { get; set; }
    }
}

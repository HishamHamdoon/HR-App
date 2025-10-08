using Emp.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Leave
{
    public class ViewLeaveDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsModified { get; set; } = false;
        public string EmployeeName { get; set; }
        public string ManagerName { get; set; }
        public string LeaveName { get; set; }
        public string Note { get; set; }
        public string? FilePath { get; set; }
    }
}

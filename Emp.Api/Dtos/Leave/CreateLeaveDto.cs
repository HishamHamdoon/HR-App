using Emp.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Leave
{
    public class CreateLeaveDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        public int EmployeeId { get; set; }
        public int LeavesTypeId { get; set; }
        public int ManagerId { get; set; }
        public bool IsHalfDay { get; set; }
        public string? Note { get; set; }
        public string? FilePath { get; set; }
        public IFormFile? Attachment { get; set; }
        public void setManager (int managerId)
        {
            this.ManagerId = managerId;
        }
    }
}

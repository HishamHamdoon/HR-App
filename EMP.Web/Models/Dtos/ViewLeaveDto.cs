using System.ComponentModel.DataAnnotations;

namespace Emp.Web.Dtos
{
    public class ViewLeaveDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsModified { get; set; } = false;
        public string EmployeeName { get; set; }
        public string ManagerName { get; set; }
        public string LeaveName { get; set; }
        public string Note { get; set; }
        public bool IsHalfDay { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }
        public string FilePath { get; set; }
    }
}

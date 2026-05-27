using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Emp.Api.Models
{
    //rename to leaves
    public class Leave
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = Utility.SD.Pending;

        // A single-day request taken as a half day (counts as 0.5 against the balance).
        public bool IsHalfDay { get; set; } = false;

        // Decision audit: who actioned the request, when, and why (reason — required on reject).
        public int? DecidedById { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsModified { get; set; } = false;

        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        public int? ManagerId { get; set; }
        [ForeignKey("ManagerId")]
        public Employee Manager { get; set; }   // 👈 self-reference

        public int LeavesTypeId { get; set; }
        [ForeignKey("LeavesTypeId")]
        public LeavesType LeavesType { get; set; }

        public string Note { get; set; }
        public string? FilePath { get; set; }
    }

}

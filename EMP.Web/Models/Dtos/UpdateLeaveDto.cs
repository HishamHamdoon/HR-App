using Emp.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Web.Dtos
{
    public class UpdateLeaveDto
    {
        //[Required]
        //public int Id { get; set; }
        //[Required]
        //public DateOnly StartDate { get; set; }
        //[Required]
        //public DateOnly? EndDate { get; set; }
        //public string Status { get; set; } 
        //public DateTime? UpdatedAt { get; set; }=DateTime.Now;
        ////public bool IsDeleted { get; set; } = false;
        //public bool IsModified { get; set; } = false;
        ////public int EmployeeId { get; set; }
        ////[ForeignKey("EmployeeId")]
        ////public int LeavesTypeId { get; set; }
        //public string? Note { get; set; }
       
            //[Required]
            public int Id { get; set; }

        //[Required]
        //public DateTime StartDate { get; set; }   // Changed from DateOnly

        //[Required]
        //public DateTime? EndDate { get; set; }    // Changed from DateOnly?

        public string Status { get; set; }

            public string? Note { get; set; }
    }
}

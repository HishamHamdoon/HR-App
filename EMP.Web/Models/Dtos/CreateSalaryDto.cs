using System.ComponentModel.DataAnnotations;

namespace EMP.Web.Models.Dtos
{
    public class CreateSalaryDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Select an Employee")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "Enter the basic salary")]
        public decimal BasicSalary { get; set; }
        //[Required(ErrorMessage = "Enter the basic Allowances")]
        public decimal? Allowances { get; set; }
        public decimal? Deductions { get; set; } = 0m;
        public DateTime? EffectiveDate { get; set; }
    }
}

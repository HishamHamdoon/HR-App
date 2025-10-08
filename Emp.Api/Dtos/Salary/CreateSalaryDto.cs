namespace Emp.Api.Dtos.Salary
{
    public class CreateSalaryDto
    {
        public int EmployeeId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    }

}

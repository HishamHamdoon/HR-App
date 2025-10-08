namespace Emp.Api.Dtos
{
    public class PayrollDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime SalaryMonth { get; set; }
        public bool IsPaid { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

}

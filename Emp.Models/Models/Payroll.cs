using Emp.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emp.Models.Models
{
    public class Payroll
    {
        public int Id { get; set; }

        // Direct reference to Employee
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        // Optional reference to the Salary used to generate this payroll
        public int? SalaryId { get; set; }
        public Salary? Salary { get; set; }

        // The month this payroll belongs to (e.g., September 2025)
        public DateTime SalaryMonth { get; set; }

        // Snapshot of salary at payroll generation time
        public decimal GrossSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }

        // Payroll status
        public bool IsPaid { get; set; }
        public DateTime GeneratedAt { get; set; }
    }


}

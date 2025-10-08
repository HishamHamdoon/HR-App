using Emp.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emp.Models
{
    public class Salary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))] // <- this marks EmployeeId as the FK

        public virtual Employee? Employee { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        [NotMapped]
        public decimal NetSalary => BasicSalary + Allowances - Deductions;

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    }
}

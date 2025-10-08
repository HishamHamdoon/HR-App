using Emp.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emp.Models.Models
{
    public class Termination
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string TerminationType { get; set; }
        public string? TerminationReason { get; set; }
        public DateOnly DateTerminated { get; set; }
        public Employee Employee { get; set; }
    }
    public enum TerminationType
    {
        Voluntary,    // Employee chooses to leave (e.g., resignation)
        Involuntary ,  // Employee is terminated by employer (e.g., layoff, firing)
        Retirement ,   // Employee retires
        ContractEnd    // Employee's contract comes to an end
    }

}

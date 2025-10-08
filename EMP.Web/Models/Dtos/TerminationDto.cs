using Emp.Api.Models;

namespace Emp.Web.Dtos
{
    public class TerminationDto
    {
        public int Id { get; set; }
        public string TerminationType { get; set; }
        public string? TerminationReason { get; set; }
        public DateOnly DateTerminated { get; set; }
        public List<Employee> Employees { get; set; }
        public int EmployeeId { get; set; }
    }
}

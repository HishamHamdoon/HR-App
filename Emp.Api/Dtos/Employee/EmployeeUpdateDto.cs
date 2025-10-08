using Emp.Api.Models;

namespace Emp.Api.Dtos.Employee
{
    public class EmployeeUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int DepartmentId { get; set; }

    }
}
//230532

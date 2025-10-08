using Emp.Api.Models;

namespace Emp.Api.Dtos.Employee
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        //public int Depid { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string employeeName { get; set; }
    }
}
//230532

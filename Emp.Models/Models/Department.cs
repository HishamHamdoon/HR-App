namespace Emp.Api.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ?Location { get; set; }
        // Department Manager
        public int? ManagerId { get; set; }
        public Employee Manager { get; set; } = null!;

        // List of employees in this department
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    }
}

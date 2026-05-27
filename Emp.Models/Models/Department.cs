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

        // Self-reference: a department can be a sub-department of another.
        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }
        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        // Sections belonging to this department.
        public ICollection<Section> Sections { get; set; } = new List<Section>();

        // List of employees in this department
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    }
}

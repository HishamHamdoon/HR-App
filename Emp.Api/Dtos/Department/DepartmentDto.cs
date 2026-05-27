namespace Emp.Api.Dtos.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public int? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }

        public List<NamedItemDto> Sections { get; set; } = new();
        public List<NamedItemDto> SubDepartments { get; set; } = new();
    }

    public class NamedItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

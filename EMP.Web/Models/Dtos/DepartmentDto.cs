namespace Emp.Web.Dtos
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public int? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }

        public List<NamedItem> Sections { get; set; } = new();
        public List<NamedItem> SubDepartments { get; set; } = new();
    }

    public class NamedItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

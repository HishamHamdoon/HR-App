namespace Emp.Web.Dtos
{
    public class DepartmentCreateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        // Department Manager
        public int? ManagerId { get; set; }
    }
}

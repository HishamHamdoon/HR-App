using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Section
{
    public class SectionCreateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
    }
}

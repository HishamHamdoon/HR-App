using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Section
{
    public class SectionUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
    }
}

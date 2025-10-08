using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Dtos.Section
{
    public class SectionViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? DepartmenName { get; set; }
    }
}

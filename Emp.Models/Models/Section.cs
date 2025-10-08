using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Models
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Models
{
    public class Qualification
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Orgnization { get; set; }
    }
}

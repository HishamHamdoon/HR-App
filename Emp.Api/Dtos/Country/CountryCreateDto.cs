using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Dtos.Models
{
    public class CountryCreateDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

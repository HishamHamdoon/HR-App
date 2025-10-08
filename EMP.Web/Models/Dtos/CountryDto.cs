using System.ComponentModel.DataAnnotations;

namespace Emp.Web.Models.Dtos
{
    public class CountryDto
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

using Emp.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace Emp.Api.Models
{
    public class Country:BaseModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        //public bool IsActive { get; set; }
        //public bool IsDeleted { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}

using Emp.Api.Data;
using Emp.Api.Models;

namespace Emp.Api.Dtos.JobTitleDto
{
    public class JobTitleViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public decimal MainSalary { get; set; } = 945000;
        //private List<Employee> Employees { get; set; }
        
    }
}

namespace Emp.Api.Dtos.JobTitleDto
{
    public class JobTitleCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public decimal MainSalary { get; set; } = 945000;
    }
}

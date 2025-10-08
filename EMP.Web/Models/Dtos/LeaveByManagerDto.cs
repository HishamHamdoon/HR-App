namespace EMP.Web.Models.Dtos
{
    public class LeaveByManagerDto
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
    }
}

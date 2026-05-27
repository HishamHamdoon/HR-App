namespace EMP.Web.Models.Dtos
{
    public class LeaveByManagerDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
    }
}

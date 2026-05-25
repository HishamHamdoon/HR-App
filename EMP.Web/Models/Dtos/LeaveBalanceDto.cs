namespace EMP.Web.Models.Dtos
{
    public class LeaveBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public int Entitlement { get; set; }
        public int Taken { get; set; }
        public int Remaining { get; set; }
    }
}

namespace EMP.Web.Models.Dtos
{
    public class LeaveBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public decimal Entitlement { get; set; }
        public decimal Taken { get; set; }
        public decimal Remaining { get; set; }
    }
}

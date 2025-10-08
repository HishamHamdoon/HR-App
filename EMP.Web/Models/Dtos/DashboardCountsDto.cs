namespace EMP.Web.Models.Dtos
{
    public class DashboardCountsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveLeaves { get; set; }
        public int Departments { get; set; }
        public int PendingApprovals { get; set; }
    }

}

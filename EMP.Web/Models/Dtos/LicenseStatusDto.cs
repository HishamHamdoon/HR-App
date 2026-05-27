namespace EMP.Web.Models.Dtos
{
    public class LicenseStatusDto
    {
        public string Type { get; set; } = "Trial";
        public DateTime ExpiresAt { get; set; }
        public bool IsValid { get; set; }
        public int DaysRemaining { get; set; }
    }
}

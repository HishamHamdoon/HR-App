namespace Emp.Api.Dtos.License
{
    public class LicenseStatusDto
    {
        public string Type { get; set; } = "Trial";
        public DateTime ExpiresAt { get; set; }
        public bool IsValid { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class ActivateLicenseDto
    {
        public string Key { get; set; } = string.Empty;
    }
}

namespace EMP.Web.Models.Dtos
{
    public class CompanySettingsDto
    {
        public string CompanyName { get; set; } = "Your Company";
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoBase64 { get; set; }
        public bool RequirePasswordChangeOnFirstLogin { get; set; } = true;
        public string DefaultCalendar { get; set; } = "Gregorian";

        /// <summary>Minutes of inactivity before the session-timeout warning appears. 0 disables it.</summary>
        public int SessionTimeoutMinutes { get; set; } = 30;
    }
}

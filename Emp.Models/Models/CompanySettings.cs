namespace Emp.Api.Models
{
    /// <summary>Single-row organisation settings used across the app and on reports.</summary>
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "Your Company";
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        /// <summary>Company logo as a base64 string (no data-URI prefix). Null = no logo.</summary>
        public string? LogoBase64 { get; set; }

        /// <summary>When true, newly created users must change their password at first login.</summary>
        public bool RequirePasswordChangeOnFirstLogin { get; set; } = true;

        /// <summary>Organisation default calendar: "Gregorian" or "Hijri".</summary>
        public string DefaultCalendar { get; set; } = "Gregorian";
    }
}

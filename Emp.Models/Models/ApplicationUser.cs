using Emp.Api.Models;
using Microsoft.AspNetCore.Identity;


namespace Emp.Models.Models
{
    

        public class ApplicationUser : IdentityUser
        {
            public int EmployeeId { get; set; }
            public Employee Employee { get; set; }

            /// <summary>When true, the user must set a new password before using the app.</summary>
            public bool MustChangePassword { get; set; } = false;

            /// <summary>Per-user UI theme: "light" or "dark" (null = use default).</summary>
            public string? PreferredTheme { get; set; }

            /// <summary>Per-user calendar: "Gregorian" or "Hijri" (null = use org default).</summary>
            public string? PreferredCalendar { get; set; }

            /// <summary>Per-user UI language: "en" or "ar" (null = default).</summary>
            public string? PreferredLanguage { get; set; }
        }
    

}

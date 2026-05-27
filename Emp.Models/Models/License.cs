namespace Emp.Api.Models
{
    /// <summary>An activated product license (a 30-day Trial or a 1-year Yearly key).</summary>
    public class License
    {
        public int Id { get; set; }

        /// <summary>"Trial" or "Yearly".</summary>
        public string Type { get; set; } = "Trial";

        /// <summary>The signed key that was activated (null for the auto-created trial).</summary>
        public string? Key { get; set; }

        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

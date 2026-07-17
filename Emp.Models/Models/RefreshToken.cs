namespace Emp.Models.Models
{
    /// <summary>
    /// A long-lived token that lets a client obtain a fresh access token without
    /// re-entering credentials. Only the SHA-256 hash of the token is stored, so a
    /// database leak does not expose usable tokens. Rotated on every use: the old row is
    /// revoked and a new one issued.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }

        /// <summary>The owning ApplicationUser (Identity) id.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>SHA-256 (base64) of the raw token. The raw value is never persisted.</summary>
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        /// <summary>Set when the token is rotated or the user signs out.</summary>
        public DateTime? RevokedAt { get; set; }
    }
}

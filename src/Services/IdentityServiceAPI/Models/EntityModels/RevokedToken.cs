using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityServiceAPI.Models.EntityModels
{
    /// <summary>
    /// A JWT that has been explicitly logged out.
    /// Identified by its "jti" (JWT ID) claim.
    /// Rows can be purged once ExpiresAt has passed — after that
    /// the token fails normal lifetime validation anyway.
    /// </summary>
    public class RevokedToken
    {
        [Key]
        public int Id { get; set; }

        /// <summary>The "jti" claim of the revoked token.</summary>
        [Required]
        [MaxLength(64)]
        public string Jti { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>Original token expiry — used to purge stale rows.</summary>
        public DateTime ExpiresAt { get; set; }

        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}

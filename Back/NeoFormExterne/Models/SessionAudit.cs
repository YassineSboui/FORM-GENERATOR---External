using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoFormExterne.Models
{
    /// <summary>
    /// Optional entity for tracking session authentication events
    /// This is used for audit logging and can be cleaned up automatically
    /// </summary>
    [Table("SessionAudits")]
    public class SessionAudit
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Client identifier
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ClientId { get; set; }

        /// <summary>
        /// Form GUID accessed
        /// </summary>
        [Required]
        public Guid Guid { get; set; }

        /// <summary>
        /// Personal access code
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PersonalCode { get; set; }

        /// <summary>
        /// Authentication type: otp, oidc, invitation, none
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string AuthType { get; set; }

        /// <summary>
        /// User email (if applicable)
        /// </summary>
        [MaxLength(255)]
        public string? Email { get; set; }

        /// <summary>
        /// OIDC user identifier (if applicable)
        /// </summary>
        [MaxLength(255)]
        public string? OidcUserId { get; set; }

        /// <summary>
        /// Session token ID (jti claim)
        /// </summary>
        [MaxLength(100)]
        public string? TokenId { get; set; }

        /// <summary>
        /// When the session was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Session expiration time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Whether the session was successfully created
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if session creation failed
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }
}

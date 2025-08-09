using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Represents a user authentication token stored in the database.
    /// Used for token management, blacklisting, and refresh token functionality.
    /// </summary>
    [Table("UserTokens")]
    public class UserToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for the token record.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with this token.
        /// </summary>
        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the JWT token string.
        /// </summary>
        [Required]
        [MaxLength(2000)]
        [Column("TokenValue")]
        public string TokenValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token type (Access, Refresh, etc.).
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("TokenType")]
        public string TokenType { get; set; } = "Access";

        /// <summary>
        /// Gets or sets when the token expires.
        /// </summary>
        [Column("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets whether the token is active (not blacklisted).
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the token has been revoked.
        /// </summary>
        [Column("IsRevoked")]
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Gets or sets the device/client information that requested the token.
        /// </summary>
        [MaxLength(500)]
        [Column("DeviceInfo")]
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the token was requested.
        /// </summary>
        [MaxLength(50)]
        [Column("IpAddress")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets when the token was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the token was last used.
        /// </summary>
        [Column("LastUsedAt")]
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Gets or sets when the token was revoked.
        /// </summary>
        [Column("RevokedAt")]
        public DateTime? RevokedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user associated with this token.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Gets whether the token is expired.
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        /// <summary>
        /// Gets whether the token is valid (active, not revoked, not expired).
        /// </summary>
        [NotMapped]
        public bool IsValid => IsActive && !IsRevoked && !IsExpired;
    }
}

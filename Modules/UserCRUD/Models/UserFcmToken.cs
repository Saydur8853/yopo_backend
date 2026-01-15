using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Represents an FCM device token associated with a user.
    /// </summary>
    [Table("UserFcmTokens")]
    public class UserFcmToken
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
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the FCM device token string.
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the token is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets when the token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the token was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this token.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using YopoBackend.Modules.UserTypeCRUD.Models;

namespace YopoBackend.Modules.InvitationCRUD.Models
{
    /// <summary>
    /// Invitation entity for module ID: 2
    /// </summary>
    public class Invitation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invitation.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person being invited.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user type ID for the invitation (foreign key to UserType).
        /// </summary>
        [Required]
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the invitation expires.
        /// </summary>
        [Required]
        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the invitation was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the invitation was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets a value indicating whether the invitation has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiryTime;

        /// <summary>
        /// Gets the number of days until the invitation expires.
        /// </summary>
        public int DaysUntilExpiry => (int)(ExpiryTime - DateTime.UtcNow).TotalDays;

        /// <summary>
        /// Navigation property to the associated user type.
        /// </summary>
        public virtual UserType UserType { get; set; } = null!;
    }
}

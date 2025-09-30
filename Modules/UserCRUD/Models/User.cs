using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Represents a user in the system with authentication and profile information.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    [Table("Users")]
    public class User : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user (used for login).
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the user.
        /// </summary>
        [MaxLength(500)]
        [Column("Address")]
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [MaxLength(20)]
        [Column("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the profile photo binary data of the user.
        /// Stores the image as binary data (LONGBLOB/VARBINARY(MAX)) in the database.
        /// </summary>
        [Column("ProfilePhoto", TypeName = "LONGBLOB")]
        public byte[]? ProfilePhoto { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the profile photo (e.g., 'image/jpeg', 'image/png').
        /// </summary>
        [MaxLength(50)]
        [Column("ProfilePhotoMimeType")]
        public string? ProfilePhotoMimeType { get; set; }

        /// <summary>
        /// Gets or sets the user type ID that determines the user's role and permissions.
        /// </summary>
        [Required]
        [Column("UserTypeId")]
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        [Column("IsEmailVerified")]
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// The ID of the inviter user when this user registered via an invitation.
        /// </summary>
        [Column("inviteById")]
        public int? InviteById { get; set; }

        /// <summary>
        /// The name of the inviter user when this user registered via an invitation.
        /// Stored redundantly for convenience/reporting.
        /// </summary>
        [MaxLength(200)]
        [Column("inviteByName")]
        public string? InviteByName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this user record.
        /// For self-registration, this will be set to the same user's ID after creation.
        /// </summary>
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the user was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user type associated with this user.
        /// </summary>
        [ForeignKey("UserTypeId")]
        public virtual UserType? UserType { get; set; }


    }
}

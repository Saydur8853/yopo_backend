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
        [Column("EmailAddress")]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [MaxLength(20)]
        [Column("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the profile photo URL/path of the user.
        /// </summary>
        [MaxLength(1000)]
        [Column("ProfilePhoto")]
        public string? ProfilePhoto { get; set; }

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
        /// Gets or sets the date and time when the user last logged in.
        /// </summary>
        [Column("LastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

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

        /// <summary>
        /// Gets the full name of the user (combination of first and last name).
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}

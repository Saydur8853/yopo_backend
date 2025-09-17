using System.ComponentModel.DataAnnotations;
using YopoBackend.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.UserTypeCRUD.Models
{
    /// <summary>
    /// Represents a user type with associated module permissions.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    public class UserType : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user type.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the user type.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the user type.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether this user type is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the ID of the user who created this user type.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets when the user type was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the user type was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the data access control type for this user type.
        /// "OWN" = Users can only access their own created data
        /// "ALL" = Users can access all data (Super Admin level)
        /// "PM" = Property Manager ecosystem isolation - users can access data within their PM's ecosystem
        /// </summary>
        [MaxLength(10)]
        public string DataAccessControl { get; set; } = "ALL";

        /// <summary>
        /// Navigation property for the user type module permissions.
        /// </summary>
        public virtual ICollection<UserTypeModulePermission> ModulePermissions { get; set; } = new List<UserTypeModulePermission>();
    }

    /// <summary>
    /// Represents the relationship between user types and modules (permissions).
    /// </summary>
    public class UserTypeModulePermission
    {
        /// <summary>
        /// Gets or sets the unique identifier for the permission.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user type ID.
        /// </summary>
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the module ID from the modules table.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets whether this permission is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets when this permission was granted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to the user type.
        /// </summary>
        public virtual UserType UserType { get; set; } = null!;

        /// <summary>
        /// Navigation property to the module.
        /// </summary>
        public virtual Module Module { get; set; } = null!;
    }
}

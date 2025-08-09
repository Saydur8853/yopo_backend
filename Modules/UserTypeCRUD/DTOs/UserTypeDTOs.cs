using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.UserTypeCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new user type.
    /// Module ID: 1 (UserTypeCRUD)
    /// </summary>
    public class CreateUserTypeDto
    {
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
        /// Gets or sets the list of module IDs this user type should have access to.
        /// </summary>
        public List<int> ModuleIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the data access control type for this user type.
        /// "OWN" = Users can only access their own created data
        /// "ALL" = Users can access all data for their user type
        /// </summary>
        public string DataAccessControl { get; set; } = "ALL";
    }

    /// <summary>
    /// DTO for updating an existing user type.
    /// </summary>
    public class UpdateUserTypeDto
    {
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
        /// Gets or sets the list of module IDs this user type should have access to.
        /// </summary>
        public List<int> ModuleIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the data access control type for this user type.
        /// "OWN" = Users can only access their own created data
        /// "ALL" = Users can access all data for their user type
        /// </summary>
        public string DataAccessControl { get; set; } = "ALL";
    }

    /// <summary>
    /// DTO for user type response data.
    /// </summary>
    public class UserTypeDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the user type.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the user type.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether this user type is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets when the user type was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the user type was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the list of module IDs this user type has access to.
        /// </summary>
        public List<int> ModuleIds { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the list of module names this user type has access to.
        /// </summary>
        public List<string> ModuleNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the data access control type for this user type.
        /// "OWN" = Users can only access their own created data
        /// "ALL" = Users can access all data for their user type
        /// </summary>
        public string DataAccessControl { get; set; } = "ALL";
    }

    /// <summary>
    /// DTO for user type module permission operations.
    /// </summary>
    public class UserTypeModulePermissionDto
    {
        /// <summary>
        /// Gets or sets the user type ID.
        /// </summary>
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the user type name.
        /// </summary>
        public string UserTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the module ID.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets the module name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this permission is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets when this permission was granted.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

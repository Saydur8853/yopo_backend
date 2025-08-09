using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Models
{
    /// <summary>
    /// Represents a module in the system with its ID, name, and metadata.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// Gets or sets the unique identifier for the module.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the module.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the module.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether the module is currently active/enabled.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of the module.
        /// </summary>
        [MaxLength(20)]
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets when the module was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the module was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

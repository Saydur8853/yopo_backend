using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.BuildingCRUD.Models
{
    /// <summary>
    /// Represents a building entity with basic information and photo.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Gets or sets the unique identifier for the building.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the photo URL or path for the building.
        /// </summary>
        [MaxLength(1000)]
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets when the building was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the building was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

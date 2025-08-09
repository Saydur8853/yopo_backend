using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.BuildingCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new building.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class CreateBuildingDto
    {
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
    }

    /// <summary>
    /// DTO for updating an existing building.
    /// </summary>
    public class UpdateBuildingDto
    {
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
    }

    /// <summary>
    /// DTO for building response data.
    /// </summary>
    public class BuildingDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the building.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the photo URL or path for the building.
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this building.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets when the building was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the building was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

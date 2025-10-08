using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.AmenityCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new amenity.
    /// </summary>
    public class CreateAmenityDTO
    {
        /// <summary>
        /// Building ID this amenity belongs to.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Name of the amenity (e.g., "Gym", "Pool").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of amenity (e.g., "Shared", "Private").
        /// </summary>
        [MaxLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Description of the amenity.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Optional floor ID if amenity is on a specific floor.
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Whether the amenity is available/operational.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Operating hours (e.g., "8 AM - 10 PM").
        /// </summary>
        [MaxLength(100)]
        public string? OpenHours { get; set; }

        /// <summary>
        /// Access control type (e.g., "QR Code", "Face Recognition").
        /// </summary>
        [MaxLength(100)]
        public string? AccessControl { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing amenity. All fields are optional.
    /// </summary>
    public class UpdateAmenityDTO
    {
        /// <summary>
        /// Name of the amenity.
        /// </summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Type of amenity.
        /// </summary>
        [MaxLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Description of the amenity.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Floor ID if amenity is on a specific floor.
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Whether the amenity is available/operational.
        /// </summary>
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// Operating hours.
        /// </summary>
        [MaxLength(100)]
        public string? OpenHours { get; set; }

        /// <summary>
        /// Access control type.
        /// </summary>
        [MaxLength(100)]
        public string? AccessControl { get; set; }
    }

    /// <summary>
    /// DTO for amenity response data.
    /// </summary>
    public class AmenityResponseDTO
    {
        /// <summary>
        /// Unique identifier for the amenity.
        /// </summary>
        public int AmenityId { get; set; }

        /// <summary>
        /// Building ID this amenity belongs to.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Name of the amenity.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of amenity.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Description of the amenity.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Floor ID if amenity is on a specific floor.
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Whether the amenity is available/operational.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Operating hours.
        /// </summary>
        public string? OpenHours { get; set; }

        /// <summary>
        /// Access control type.
        /// </summary>
        public string? AccessControl { get; set; }

        /// <summary>
        /// Date and time when the amenity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the amenity was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
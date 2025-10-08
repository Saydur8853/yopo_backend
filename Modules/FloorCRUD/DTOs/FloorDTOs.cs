using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.FloorCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new floor.
    /// </summary>
    public class CreateFloorDTO
    {
        /// <summary>
        /// Building ID this floor belongs to.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Floor name (e.g., "Ground Floor").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Floor number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Floor type (Residential, Commercial, etc.).
        /// </summary>
        [MaxLength(100)]
        public string? Type { get; set; }

        /// <summary>
        /// Total units on the floor.
        /// </summary>
        public int TotalUnits { get; set; }

        /// <summary>
        /// Total area in square feet.
        /// </summary>
        public decimal? AreaSqFt { get; set; }

        /// <summary>
        /// Whether floor is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Status string.
        /// </summary>
        [MaxLength(50)]
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing floor. All fields optional.
    /// </summary>
    public class UpdateFloorDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }
        public int? Number { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
        public int? TotalUnits { get; set; }
        public decimal? AreaSqFt { get; set; }
        public bool? IsActive { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO returned for floor responses.
    /// </summary>
    public class FloorResponseDTO
    {
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string? Type { get; set; }
        public int TotalUnits { get; set; }
        public decimal? AreaSqFt { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
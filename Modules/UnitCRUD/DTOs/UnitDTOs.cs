using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.UnitCRUD.DTOs
{
    public class CreateUnitDTO
    {
        [Required]
        public int FloorId { get; set; }

        [Required]
        public int BuildingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Type { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public decimal? AreaSqFt { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public int? OwnerId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasBalcony { get; set; }
        public bool HasParking { get; set; }
    }

    public class UpdateUnitDTO
    {
        [MaxLength(50)]
        public string? UnitNumber { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
        [MaxLength(50)]
        public string? Category { get; set; }
        public decimal? AreaSqFt { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public int? TenantId { get; set; }
        public int? OwnerId { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasBalcony { get; set; }
        public bool? HasParking { get; set; }
        public List<string>? Amenities { get; set; }
    }

    public class UnitResponseDTO
    {
        public int UnitId { get; set; }
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Category { get; set; }
        public decimal? AreaSqFt { get; set; }
        public string? Status { get; set; }
        public int? TenantId { get; set; }
        public int? OwnerId { get; set; }
        public bool IsActive { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasParking { get; set; }
        public List<string>? Amenities { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
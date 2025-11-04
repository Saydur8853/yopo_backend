using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.IntercomCRUD.DTOs
{
    public class CreateIntercomDTO
    {
        [Required]
        [MaxLength(200)]
        public string IntercomName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IntercomModel { get; set; }

        [MaxLength(50)]
        public string? IntercomType { get; set; }

        public decimal? Price { get; set; }
        public bool IsInstalled { get; set; }

        [MaxLength(50)]
        public string? IntercomSize { get; set; }

        [MaxLength(50)]
        public string? IntercomColor { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? DateInstalled { get; set; }
        public DateTime? ServiceDate { get; set; }

        [MaxLength(50)]
        public string? OperatingSystem { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstalledLocation { get; set; } = string.Empty; // e.g., "Main Entrance", "Parking", "Pool Entrance"

        public bool HasCCTV { get; set; }
        public bool HasPinPad { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int BuildingId { get; set; }

        // Optional - if intercom is for a specific amenity
        public int? AmenityId { get; set; }
    }

    public class UpdateIntercomDTO
    {
        [MaxLength(200)]
        public string? IntercomName { get; set; }
        [MaxLength(100)]
        public string? IntercomModel { get; set; }
        [MaxLength(50)]
        public string? IntercomType { get; set; }
        public decimal? Price { get; set; }
        public bool? IsInstalled { get; set; }
        [MaxLength(50)]
        public string? IntercomSize { get; set; }
        [MaxLength(50)]
        public string? IntercomColor { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateInstalled { get; set; }
        public DateTime? ServiceDate { get; set; }
        [MaxLength(50)]
        public string? OperatingSystem { get; set; }
        [MaxLength(200)]
        public string? InstalledLocation { get; set; }
        public bool? HasCCTV { get; set; }
        public bool? HasPinPad { get; set; }
        public int? CustomerId { get; set; }
        public int? BuildingId { get; set; }
        public int? AmenityId { get; set; }
    }


    public class CustomerInfoDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
    }

    public class BuildingInfoDTO
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string BuildingAddress { get; set; } = string.Empty;
    }

    public class FloorInfoDTO
    {
        public int FloorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
    }

    public class AmenityInfoDTO
    {
        public int AmenityId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public string? AmenityType { get; set; }
    }

    public class IntercomResponseDTO
    {
        public int IntercomId { get; set; }
        public string IntercomName { get; set; } = string.Empty;
        public string? IntercomModel { get; set; }
        public string? IntercomType { get; set; }
        public decimal? Price { get; set; }
        public bool IsInstalled { get; set; }
        public string? IntercomSize { get; set; }
        public string? IntercomColor { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateInstalled { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string? OperatingSystem { get; set; }
        public string? InstalledLocation { get; set; }
        public bool HasCCTV { get; set; }
        public bool HasPinPad { get; set; }
        public int CustomerId { get; set; }
        public int BuildingId { get; set; }
        public int? AmenityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Expanded info
        public CustomerInfoDTO? Customer { get; set; }
        public BuildingInfoDTO? Building { get; set; }
        public AmenityInfoDTO? Amenity { get; set; }
    }
}
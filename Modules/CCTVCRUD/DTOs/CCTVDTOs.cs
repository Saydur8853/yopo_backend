using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.CCTVCRUD.DTOs
{
    public class CCTVResponseDto
    {
        public int CCTVId { get; set; }
        public string CCTVName { get; set; } = string.Empty;
        public string? StreamUrl { get; set; }
        public string? BuildingName { get; set; }
        public string? BuildingAddress { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCCTVDto
    {
        [Required]
        [MaxLength(200)]
        public string CCTVName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? StreamUrl { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int BuildingId { get; set; }
        
        public int? CustomerId { get; set; }
    }

    public class UpdateCCTVDto
    {
        [MaxLength(200)]
        public string? CCTVName { get; set; }

        [MaxLength(500)]
        public string? StreamUrl { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }
        
        public bool? IsActive { get; set; }
    }
}

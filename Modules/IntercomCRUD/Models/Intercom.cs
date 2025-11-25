using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.AmenityCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.IntercomCRUD.Models
{
    [Table("Intercoms")]
    public class Intercom
    {
        [Key]
        public int IntercomId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("IntercomName")]
        public string IntercomName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("IntercomModel")]
        public string? IntercomModel { get; set; }

        [MaxLength(50)]
        [Column("IntercomType")]
        public string? IntercomType { get; set; }

        [Column("Price")]
        public decimal? Price { get; set; }

        [Column("IsInstalled")]
        public bool IsInstalled { get; set; }

        [MaxLength(50)]
        [Column("IntercomSize")]
        public string? IntercomSize { get; set; }

        [MaxLength(50)]
        [Column("IntercomColor")]
        public string? IntercomColor { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("DateInstalled")]
        public DateTime? DateInstalled { get; set; }

        [Column("ServiceDate")]
        public DateTime? ServiceDate { get; set; }

        [MaxLength(50)]
        [Column("OperatingSystem")]
        public string? OperatingSystem { get; set; }

        [MaxLength(200)]
        [Column("InstalledLocation")]
        public string? InstalledLocation { get; set; }

        [Column("HasCCTV")]
        public bool HasCCTV { get; set; }

        [MaxLength(500)]
        [Column("StreamUrl")]
        public string? StreamUrl { get; set; }

        [Column("HasPinPad")]
        public bool HasPinPad { get; set; }

        [Required]
        [Column("CustomerId")]
        public int CustomerId { get; set; }

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [Column("AmenityId")]
        public int? AmenityId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [Column("CreatedBy")]
        public int? CreatedBy { get; set; }

        [Column("UpdatedBy")]
        public int? UpdatedBy { get; set; }

        // Navigation
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [ForeignKey("AmenityId")]
        public virtual Amenity? Amenity { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }
}

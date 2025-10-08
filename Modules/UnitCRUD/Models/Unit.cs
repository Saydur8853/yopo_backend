using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.FloorCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.UnitCRUD.Models
{
    /// <summary>
    /// Represents a unit (flat, office, shop, etc.) within a floor and building.
    /// Module: UnitCRUD
    /// </summary>
    [Table("Units")]
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        [Required]
        [Column("FloorId")]
        public int FloorId { get; set; }

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("UnitNumber")]
        public string UnitNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("Type")]
        public string? Type { get; set; }

        [MaxLength(50)]
        [Column("Category")]
        public string? Category { get; set; }

        [Column("AreaSqFt")]
        public decimal? AreaSqFt { get; set; }

        [MaxLength(50)]
        [Column("Status")]
        public string? Status { get; set; }

        [Column("TenantId")]
        public int? TenantId { get; set; }

        [Column("OwnerId")]
        public int? OwnerId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("HasBalcony")]
        public bool HasBalcony { get; set; }

        [Column("HasParking")]
        public bool HasParking { get; set; }

        /// <summary>
        /// JSON array of amenities for this unit.
        /// </summary>
        [Column("Amenities", TypeName = "json")]
        public string? Amenities { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("FloorId")]
        public virtual Floor Floor { get; set; } = null!;

        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [ForeignKey("TenantId")]
        public virtual User? Tenant { get; set; }

        [ForeignKey("OwnerId")]
        public virtual User? Owner { get; set; }
    }
}
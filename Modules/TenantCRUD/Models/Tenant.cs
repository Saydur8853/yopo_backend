using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.FloorCRUD.Models;
using YopoBackend.Modules.UnitCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.TenantCRUD.Models
{
    /// <summary>
    /// Represents a tenant associated with a building/floor/unit.
    /// </summary>
    [Table("Tenants")]
    public class Tenant : ICreatedByEntity
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("TenantName")]
        public string TenantName { get; set; } = string.Empty;

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [MaxLength(100)]
        [Column("Type")] // e.g., Residential, Commercial
        public string? Type { get; set; }

        [Column("FloorId")]
        public int? FloorId { get; set; }

        [Column("UnitId")]
        public int? UnitId { get; set; }

        [Column("ContractStartDate")]
        public DateTime? ContractStartDate { get; set; }

        [Column("ContractEndDate")]
        public DateTime? ContractEndDate { get; set; }

        [Column("IsPaid")]
        public bool IsPaid { get; set; }

        [MaxLength(100)]
        [Column("MemberType")] // e.g., Owner, Renter, Company, FamilyMember
        public string? MemberType { get; set; }

        [MaxLength(1000)]
        [Column("DocumentFile")] // Path or URL to the document
        public string? DocumentFile { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [ForeignKey("FloorId")]
        public virtual Floor? Floor { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.VirtualKeyCRUD.Models
{
    /// <summary>
    /// Represents a virtual key in the system for access control.
    /// Module: VirtualKeyCRUD (Module ID: 10)
    /// </summary>
    [Table("VirtualKeys")]
    public class VirtualKey : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the virtual key.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("KeyId")]
        public int KeyId { get; set; }

        /// <summary>
        /// Gets or sets the description of the virtual key.
        /// </summary>
        [StringLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the virtual key was created.
        /// </summary>
        [Required]
        [Column("DateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the type of the virtual key (e.g., Temporary, Permanent, Emergency).
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("Type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the virtual key (e.g., Active, Inactive, Expired).
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets the expiration date of the virtual key.
        /// </summary>
        [Column("DateExpired")]
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Gets or sets the access location for the virtual key.
        /// </summary>
        [StringLength(500)]
        [Column("AccessLocation")]
        public string? AccessLocation { get; set; }

        /// <summary>
        /// Gets or sets the building ID that this virtual key provides access to.
        /// </summary>
        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the intercom ID associated with this virtual key.
        /// </summary>
        [Column("IntercomId")]
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with this virtual key.
        /// </summary>
        [Column("TenantId")]
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the PIN code for the virtual key.
        /// </summary>
        [StringLength(20)]
        [Column("PinCode")]
        public string? PinCode { get; set; }

        /// <summary>
        /// Gets or sets the QR code data for the virtual key.
        /// </summary>
        [StringLength(1000)]
        [Column("QrCode")]
        public string? QrCode { get; set; }

        /// <summary>
        /// Gets or sets whether the virtual key is active.
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the ID of the user who created this virtual key record.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [Required]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property to the building this virtual key provides access to.
        /// </summary>
        [ForeignKey(nameof(BuildingId))]
        public virtual YopoBackend.Modules.BuildingCRUD.Models.Building? Building { get; set; }

        /// <summary>
        /// Navigation property to the tenant associated with this virtual key.
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual YopoBackend.Modules.TenantCRUD.Models.Tenant? Tenant { get; set; }

        /// <summary>
        /// Navigation property to the intercom associated with this virtual key.
        /// </summary>
        [ForeignKey(nameof(IntercomId))]
        public virtual YopoBackend.Modules.IntercomCRUD.Models.Intercom? Intercom { get; set; }
    }
}

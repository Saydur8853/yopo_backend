using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.TenantCRUD.Models
{
    /// <summary>
    /// Represents a tenant in the system.
    /// Module: TenantCRUD (Module ID: 5)
    /// </summary>
    [Table("Tenants")]
    public class Tenant
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tenant.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant name.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the building ID that this tenant belongs to.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the tenant type (e.g., Residential, Commercial, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the floor number.
        /// </summary>
        [Required]
        public int Floor { get; set; }

        /// <summary>
        /// Gets or sets the unit number.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string UnitNo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of parking spaces allocated.
        /// </summary>
        public int ParkingSpace { get; set; }

        /// <summary>
        /// Gets or sets the contact information.
        /// </summary>
        [StringLength(500)]
        public string? Contact { get; set; }

        /// <summary>
        /// Gets or sets the contract start date.
        /// </summary>
        [Required]
        public DateTime ContractStartDate { get; set; }

        /// <summary>
        /// Gets or sets the contract end date.
        /// </summary>
        [Required]
        public DateTime ContractEndDate { get; set; }

        /// <summary>
        /// Gets or sets whether the tenant has paid their dues.
        /// </summary>
        public bool Paid { get; set; }

        /// <summary>
        /// Gets or sets the member type (e.g., Owner, Renter, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string MemberType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file paths or document references (JSON format).
        /// </summary>
        [Column(TypeName = "json")]
        public string? Files { get; set; }

        /// <summary>
        /// Gets or sets whether the tenant record is active.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the building this tenant belongs to.
        /// </summary>
        [ForeignKey(nameof(BuildingId))]
        public virtual YopoBackend.Modules.BuildingCRUD.Models.Building? Building { get; set; }
    }
}

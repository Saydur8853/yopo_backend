using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.TenantCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomAccessCodes")]
    public class IntercomAccessCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [Column("IntercomId")]
        public int? IntercomId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("CodeType")] // QR or PIN
        public string CodeType { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("CodeHash")]
        public string CodeHash { get; set; } = string.Empty;

        // TEMPORARY: store raw PIN so it can be shown via API; consider removing or encrypting later.
        [MaxLength(200)]
        [Column("CodePlain")]
        public string? CodePlain { get; set; }

        // Optional tenant owner for this access code
        [Column("TenantId")]
        public int? TenantId { get; set; }

        [Column("ExpiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Building Building { get; set; } = null!;

        public virtual Intercom? Intercom { get; set; }

        public virtual User CreatedByUser { get; set; } = null!;

        public virtual Tenant? Tenant { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomMasterPins")]
    public class IntercomMasterPin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("IntercomId")]
        public int IntercomId { get; set; }

        // Store only hashed pins
        [Required]
        [MaxLength(255)]
        [Column("PinHash")]
        public string PinHash { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedBy")]
        public int? UpdatedBy { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IntercomId")]
        public virtual Intercom Intercom { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomTemporaryPins")]
    public class IntercomTemporaryPin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("IntercomId")]
        public int IntercomId { get; set; }

        [Required]
        [Column("CreatedByUserId")]
        public int CreatedByUserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("PinHash")]
        public string PinHash { get; set; } = string.Empty;

        [Column("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Column("MaxUses")]
        public int MaxUses { get; set; } = 1;

        [Column("UsesCount")]
        public int UsesCount { get; set; } = 0;

        [Column("FirstUsedAt")]
        public DateTime? FirstUsedAt { get; set; }

        [Column("LastUsedAt")]
        public DateTime? LastUsedAt { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("IntercomId")]
        public virtual Intercom Intercom { get; set; } = null!;

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
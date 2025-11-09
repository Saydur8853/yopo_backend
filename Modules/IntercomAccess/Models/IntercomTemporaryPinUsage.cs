using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomTemporaryPinUsages")]
    public class IntercomTemporaryPinUsage
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("TemporaryPinId")]
        public int TemporaryPinId { get; set; }

        [Column("UsedAt")]
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        [Column("UsedFromIp")]
        [MaxLength(50)]
        public string? UsedFromIp { get; set; }

        [Column("DeviceInfo")]
        [MaxLength(200)]
        public string? DeviceInfo { get; set; }

        [ForeignKey("TemporaryPinId")]
        public virtual IntercomTemporaryPin TemporaryPin { get; set; } = null!;
    }
}
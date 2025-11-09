using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.IntercomCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomAccessLogs")]
    public class IntercomAccessLog
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("IntercomId")]
        public int IntercomId { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [MaxLength(20)]
        [Column("CredentialType")] // Master, User, Temporary
        public string CredentialType { get; set; } = string.Empty;

        [Column("CredentialRefId")] // Id of IntercomMasterPin/IntercomUserPin/IntercomTemporaryPin if applicable
        public int? CredentialRefId { get; set; }

        [Column("IsSuccess")]
        public bool IsSuccess { get; set; }

        [MaxLength(200)]
        [Column("Reason")] // e.g., Expired, Invalid, MaxUsesReached
        public string? Reason { get; set; }

        [Column("OccurredAt")]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        [Column("IpAddress")]
        public string? IpAddress { get; set; }

        [MaxLength(200)]
        [Column("DeviceInfo")]
        public string? DeviceInfo { get; set; }

        [ForeignKey("IntercomId")]
        public virtual Intercom Intercom { get; set; } = null!;
    }
}
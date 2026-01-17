using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.VerifyIdentity.Models
{
    [Table("IdentityVerificationRequests")]
    public class IdentityVerificationRequest : ICreatedByEntity
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int TenantId { get; set; }

        public int? BuildingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = IdentityVerificationStatus.Pending;

        public int CreatedBy { get; set; }

        public int? VerifiedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("TenantId")]
        public virtual User? TenantUser { get; set; }

        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("VerifiedBy")]
        public virtual User? VerifiedByUser { get; set; }

        public virtual ICollection<IdentityVerificationDocument> Documents { get; set; } = new List<IdentityVerificationDocument>();
    }

    [Table("IdentityVerificationDocuments")]
    public class IdentityVerificationDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        public string DocumentUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? DocumentPublicId { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RequestId")]
        public virtual IdentityVerificationRequest Request { get; set; } = null!;
    }

    public static class IdentityVerificationStatus
    {
        public const string Pending = "Pending";
        public const string Done = "Done";
    }
}

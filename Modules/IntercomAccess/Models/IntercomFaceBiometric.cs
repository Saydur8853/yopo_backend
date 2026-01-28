using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("IntercomFaceBiometrics")]
    public class IntercomFaceBiometric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(2048)]
        [Column("FrontImageUrl")]
        public string FrontImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("FrontImagePublicId")]
        public string FrontImagePublicId { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        [Column("LeftImageUrl")]
        public string LeftImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("LeftImagePublicId")]
        public string LeftImagePublicId { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        [Column("RightImageUrl")]
        public string RightImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("RightImagePublicId")]
        public string RightImagePublicId { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("FrontImageHash")]
        public string FrontImageHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("LeftImageHash")]
        public string LeftImageHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("RightImageHash")]
        public string RightImageHash { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("FrontImageMimeType")]
        public string? FrontImageMimeType { get; set; }

        [MaxLength(50)]
        [Column("LeftImageMimeType")]
        public string? LeftImageMimeType { get; set; }

        [MaxLength(50)]
        [Column("RightImageMimeType")]
        public string? RightImageMimeType { get; set; }

        [MaxLength(20)]
        [Column("DevicePlatform")]
        public string? DevicePlatform { get; set; }

        [MaxLength(100)]
        [Column("DeviceModel")]
        public string? DeviceModel { get; set; }

        [MaxLength(50)]
        [Column("AppVersion")]
        public string? AppVersion { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

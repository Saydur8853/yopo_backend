using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.IntercomAccess.Models
{
    [Table("temp_intercom")]
    public class TempIntercom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("TenantId")]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("TenantName")]
        public string TenantName { get; set; } = string.Empty;

        [Column("UnitId")]
        public int? UnitId { get; set; }

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [Required]
        [MaxLength(2048)]
        [Column("FrontImageUrl")]
        public string FrontImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        [Column("LeftImageUrl")]
        public string LeftImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        [Column("RightImageUrl")]
        public string RightImageUrl { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}

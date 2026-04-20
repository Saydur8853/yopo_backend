using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.Energy.Models
{
    [Table("Energy_Alerts")]
    public class EnergyAlert
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LocationId { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Severity { get; set; } = "warning";

        [Required]
        [MaxLength(50)]
        public string System { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Equipment { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public bool Acknowledged { get; set; }

        [MaxLength(100)]
        public string? AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        public int EventCount { get; set; } = 1;

        [MaxLength(200)]
        public string? BmsReference { get; set; }

        [MaxLength(500)]
        public string? RecommendedAction { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedSavingsAedMonth { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(LocationId))]
        public EnergyLocation Location { get; set; } = null!;
    }
}

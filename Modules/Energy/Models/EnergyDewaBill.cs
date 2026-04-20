using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.Energy.Models
{
    [Table("Energy_DewaBills")]
    public class EnergyDewaBill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationId { get; set; } = string.Empty;

        [Required]
        public int MeterId { get; set; }

        [Required]
        [MaxLength(7)]
        public string BillMonth { get; set; } = string.Empty;

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public int Kwh { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ElectricityAed { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        public decimal? WaterCubicMeters { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? WaterAed { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? SewerageAed { get; set; }

        public int? MeterReadingPrevious { get; set; }

        public int? MeterReadingCurrent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(LocationId))]
        public EnergyLocation Location { get; set; } = null!;

        [ForeignKey(nameof(MeterId))]
        public EnergyDewaMeter Meter { get; set; } = null!;
    }
}

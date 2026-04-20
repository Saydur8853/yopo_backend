using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.Energy.Models
{
    [Table("Energy_DewaMeters")]
    public class EnergyDewaMeter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string PremiseLabel { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? MeterNumber { get; set; }

        public decimal? MultiplicationFactor { get; set; }

        [MaxLength(20)]
        public string? CtRatio { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool HasWater { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(LocationId))]
        public EnergyLocation Location { get; set; } = null!;

        public ICollection<EnergyDewaBill> Bills { get; set; } = new List<EnergyDewaBill>();
    }
}

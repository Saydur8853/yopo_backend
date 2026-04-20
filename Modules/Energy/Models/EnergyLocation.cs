using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;

namespace YopoBackend.Modules.Energy.Models
{
    [Table("Energy_Locations")]
    public class EnergyLocation
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        public int? BuildingId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = "Dubai";

        public int TotalUnits { get; set; }

        [MaxLength(500)]
        public string? Towers { get; set; }

        public int Floors { get; set; }

        public int Basements { get; set; }

        [MaxLength(200)]
        public string? BmsType { get; set; }

        [MaxLength(100)]
        public string? GatewayId { get; set; }

        public int OccupancyPercent { get; set; } = 100;

        [MaxLength(20)]
        public string Status { get; set; } = "onboarding";

        public decimal DesignDeltaT { get; set; } = 5.5m;

        public DateTime? ConnectedSince { get; set; }

        public DateTime? LastDataReceived { get; set; }

        [MaxLength(100)]
        public string? MqttTopicPrefix { get; set; }

        [MaxLength(100)]
        public string? InfluxBucket { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(BuildingId))]
        public Building? Building { get; set; }

        public ICollection<EnergyDewaMeter> DewaMeters { get; set; } = new List<EnergyDewaMeter>();
        public ICollection<EnergyAlert> Alerts { get; set; } = new List<EnergyAlert>();
    }
}

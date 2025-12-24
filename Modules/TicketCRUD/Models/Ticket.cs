using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.UnitCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.TicketCRUD.Models
{
    [Table("Tickets")]
    public class Ticket : ICreatedByEntity
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = TicketStatus.New;

        [Required]
        [MaxLength(50)]
        public string ConcernLevel { get; set; } = TicketConcernLevel.Normal;

        [MaxLength(100)]
        public string? TimeFrame { get; set; }

        [MaxLength(200)]
        public string? ServicePerson { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [Required]
        public int BuildingId { get; set; }

        public int? UnitId { get; set; }

        public int? TenantUserId { get; set; }

        public bool IsDeleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime? StatusUpdatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        [ForeignKey("TenantUserId")]
        public virtual User? TenantUser { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("DeletedBy")]
        public virtual User? DeletedByUser { get; set; }
    }

    public static class TicketStatus
    {
        public const string New = "New";
        public const string Accepted = "Accepted";
        public const string Investigating = "Investigating";
        public const string TimeFrameSet = "TimeFrameSet";
        public const string ServiceManSent = "ServiceManSent";
        public const string Feedback = "Feedback";
        public const string Done = "Done";
        public const string Rejected = "Rejected";
    }

    public static class TicketConcernLevel
    {
        public const string Low = "Low";
        public const string Normal = "Normal";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Urgent = "Urgent";
    }
}

using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.TicketCRUD.DTOs
{
    public class TicketResponseDTO
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ConcernLevel { get; set; } = string.Empty;
        public string? TimeFrame { get; set; }
        public string? ServicePerson { get; set; }
        public string? Feedback { get; set; }
        public string? RejectionReason { get; set; }
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public int? UnitId { get; set; }
        public string? UnitNumber { get; set; }
        public int? TenantUserId { get; set; }
        public string? TenantName { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
    }

    public class TicketStatusChangedDTO
    {
        public int TicketId { get; set; }
        public int BuildingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StatusUpdatedAt { get; set; }
    }

    public class CreateTicketDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int? BuildingId { get; set; }

        public int? UnitId { get; set; }

        [MaxLength(50)]
        public string? ConcernLevel { get; set; }
    }

    public class UpdateTicketDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? ConcernLevel { get; set; }

        [MaxLength(100)]
        public string? TimeFrame { get; set; }

        [MaxLength(200)]
        public string? ServicePerson { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public int? UnitId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.AnnouncementCRUD.DTOs
{
    public class CreateAnnouncementDTO
    {
        [Required]
        public int BuildingId { get; set; }

        [MaxLength(200)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        public DateTime? AnnouncementDate { get; set; }

        [MaxLength(20)]
        public string? AnnouncementTime { get; set; }
    }

    public class UpdateAnnouncementDTO
    {
        [MaxLength(200)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        public DateTime? AnnouncementDate { get; set; }

        [MaxLength(20)]
        public string? AnnouncementTime { get; set; }
    }

    public class AnnouncementResponseDTO
    {
        public int AnnouncementId { get; set; }
        public int BuildingId { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public DateTime? AnnouncementDate { get; set; }
        public string? AnnouncementTime { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AnnouncementListResponseDTO
    {
        public List<AnnouncementResponseDTO> Announcements { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}

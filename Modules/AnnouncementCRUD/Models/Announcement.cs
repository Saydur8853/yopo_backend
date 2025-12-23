using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.AnnouncementCRUD.Models
{
    [Table("Announcements")]
    public class Announcement : ICreatedByEntity
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        public int BuildingId { get; set; }

        [MaxLength(200)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        public DateTime? AnnouncementDate { get; set; }

        [MaxLength(20)]
        public string? AnnouncementTime { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

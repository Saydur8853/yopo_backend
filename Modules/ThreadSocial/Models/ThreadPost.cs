using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.ThreadSocial.Models
{
    [Table("ThreadPosts")]
    public class ThreadPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AuthorType { get; set; } = "Tenant";

        public string? Content { get; set; }

        [MaxLength(2048)]
        public string? ImageUrl { get; set; }

        [MaxLength(255)]
        public string? ImagePublicId { get; set; }

        public int? BuildingId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

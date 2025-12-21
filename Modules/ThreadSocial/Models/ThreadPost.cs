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

        [Column(TypeName = "LONGBLOB")]
        public byte[]? Image { get; set; }

        [MaxLength(100)]
        public string? ImageMimeType { get; set; }

        public int? BuildingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

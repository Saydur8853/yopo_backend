using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.ThreadSocial.Models
{
    [Table("ThreadComments")]
    public class ThreadComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AuthorType { get; set; } = "Tenant";

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.Messaging.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SenderType { get; set; } = string.Empty; // 'Tenant' or 'User'

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReceiverType { get; set; } = string.Empty; // 'Tenant' or 'User'

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? BuildingId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

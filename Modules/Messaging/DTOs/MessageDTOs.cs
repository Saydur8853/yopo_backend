using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.Messaging.DTOs
{
    public class SendMessageDTO
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [RegularExpression("^(Tenant|User)$", ErrorMessage = "ReceiverType must be 'Tenant' or 'User'.")]
        public string ReceiverType { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? BuildingId { get; set; }
    }

    public class UpdateMessageDTO
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponseDTO
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public int ReceiverId { get; set; }
        public string ReceiverType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? BuildingId { get; set; }
        
        // Optional: Include sender/receiver names if needed for UI
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
    }
}

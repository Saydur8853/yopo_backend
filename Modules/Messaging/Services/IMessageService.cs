using YopoBackend.Modules.Messaging.DTOs;
using YopoBackend.Modules.Messaging.Models;

namespace YopoBackend.Modules.Messaging.Services
{
    public interface IMessageService
    {
        Task<MessageResponseDTO> SendMessageAsync(int senderId, string senderType, SendMessageDTO messageDto);
        Task<IEnumerable<MessageResponseDTO>> GetMessagesAsync(int userId, string userType);
        Task<MessageResponseDTO> UpdateMessageAsync(int messageId, int userId, string userType, UpdateMessageDTO messageDto);
        Task<int> DeleteChatAsync(int userId, string userType, int withUserId, string withUserType);
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Hubs;
using YopoBackend.Modules.Messaging.DTOs;
using YopoBackend.Modules.Messaging.Models;

namespace YopoBackend.Modules.Messaging.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessageService(ApplicationDbContext context, IHubContext<MessageHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<MessageResponseDTO> SendMessageAsync(int senderId, string senderType, SendMessageDTO messageDto)
        {
            var message = new Message
            {
                SenderId = senderId,
                SenderType = senderType,
                ReceiverId = messageDto.ReceiverId,
                ReceiverType = messageDto.ReceiverType,
                Content = messageDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var responseDto = MapToDTO(message);

            // Broadcast to receiver
            // Assuming clients join groups named "User_{UserId}" or "Tenant_{TenantId}"
            var receiverGroup = $"{messageDto.ReceiverType}_{messageDto.ReceiverId}";
            await _hubContext.Clients.Group(receiverGroup).SendAsync("ReceiveMessage", responseDto);
            
            // Also send back to sender so they see it immediately (optional if frontend handles optimistic UI)
             var senderGroup = $"{senderType}_{senderId}";
             await _hubContext.Clients.Group(senderGroup).SendAsync("ReceiveMessage", responseDto);


            return responseDto;
        }

        public async Task<IEnumerable<MessageResponseDTO>> GetMessagesAsync(int userId, string userType)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.SenderType == userType) || 
                            (m.ReceiverId == userId && m.ReceiverType == userType))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(MapToDTO);
        }

        public async Task<MessageResponseDTO> UpdateMessageAsync(int messageId, int userId, string userType, UpdateMessageDTO messageDto)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
            {
                throw new KeyNotFoundException("Message not found.");
            }

            if (message.SenderId != userId || message.SenderType != userType)
            {
                throw new UnauthorizedAccessException("You can only edit your own messages.");
            }

            message.Content = messageDto.Content;
            message.UpdatedAt = DateTime.UtcNow;

            _context.Messages.Update(message);
            await _context.SaveChangesAsync();

            var responseDto = MapToDTO(message);

            // Notify both parties about the update
            var receiverGroup = $"{message.ReceiverType}_{message.ReceiverId}";
            var senderGroup = $"{message.SenderType}_{message.SenderId}";

            await _hubContext.Clients.Group(receiverGroup).SendAsync("MessageUpdated", responseDto);
            await _hubContext.Clients.Group(senderGroup).SendAsync("MessageUpdated", responseDto);

            return responseDto;
        }

        public async Task<int> DeleteChatAsync(int userId, string userType, int withUserId, string withUserType)
        {
            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == userId && m.SenderType == userType && m.ReceiverId == withUserId && m.ReceiverType == withUserType) ||
                    (m.SenderId == withUserId && m.SenderType == withUserType && m.ReceiverId == userId && m.ReceiverType == userType))
                .ToListAsync();

            if (messages.Count == 0) return 0;

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return messages.Count;
        }

        private MessageResponseDTO MapToDTO(Message message)
        {
            return new MessageResponseDTO
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderType = message.SenderType,
                ReceiverId = message.ReceiverId,
                ReceiverType = message.ReceiverType,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt
            };
        }
    }
}

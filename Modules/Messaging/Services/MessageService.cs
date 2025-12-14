using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            var resolvedReceiver = await ResolveTenantReceiverAsync(messageDto.ReceiverId, messageDto.ReceiverType);

            var message = new Message
            {
                SenderId = senderId,
                SenderType = senderType,
                ReceiverId = resolvedReceiver.id,
                ReceiverType = resolvedReceiver.type,
                Content = messageDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var nameLookup = await BuildNameLookupAsync(new[] { message });
            var responseDto = MapToDTO(message, nameLookup);

            // Broadcast to receiver
            // Assuming clients join groups named "User_{UserId}" or "Tenant_{TenantId}"
            var receiverGroup = $"{resolvedReceiver.type}_{resolvedReceiver.id}";
            await _hubContext.Clients.Group(receiverGroup).SendAsync("ReceiveMessage", responseDto);
            
            // Also send back to sender so they see it immediately (optional if frontend handles optimistic UI)
             var senderGroup = $"{senderType}_{senderId}";
             await _hubContext.Clients.Group(senderGroup).SendAsync("ReceiveMessage", responseDto);


            return responseDto;
        }

        public async Task<IEnumerable<MessageResponseDTO>> GetMessagesAsync(int userId, string userType)
        {
            var tenantRecordIdsForUser = await GetTenantRecordIdsForUserAsync(userId);

            var messages = await _context.Messages
                .Where(m => m.SenderId > 0 && m.ReceiverId > 0) // skip malformed seed/test rows
                .Where(m =>
                    (m.SenderId == userId && m.SenderType == userType) || 
                    (m.ReceiverId == userId && m.ReceiverType == userType) ||
                    // Legacy messages saved against tenant record IDs instead of tenant user IDs
                    (tenantRecordIdsForUser.Contains(m.ReceiverId) && m.ReceiverType == "Tenant"))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var nameLookup = await BuildNameLookupAsync(messages);
            return messages.Select(m => MapToDTO(m, nameLookup));
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

            var nameLookup = await BuildNameLookupAsync(new[] { message });
            var responseDto = MapToDTO(message, nameLookup);

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

        private MessageResponseDTO MapToDTO(Message message, IDictionary<string, string>? nameLookup = null)
        {
            var senderKey = NameKey(message.SenderType, message.SenderId);
            var receiverKey = NameKey(message.ReceiverType, message.ReceiverId);

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
                UpdatedAt = message.UpdatedAt,
                SenderName = nameLookup != null && nameLookup.TryGetValue(senderKey, out var sName) ? sName : null,
                ReceiverName = nameLookup != null && nameLookup.TryGetValue(receiverKey, out var rName) ? rName : null
            };
        }

        private async Task<Dictionary<string, string>> BuildNameLookupAsync(IEnumerable<Message> messages)
        {
            var keys = messages
                .SelectMany(m => new[]
                {
                    (Type: m.SenderType, Id: m.SenderId),
                    (Type: m.ReceiverType, Id: m.ReceiverId)
                })
                .Where(k => k.Id > 0 && !string.IsNullOrWhiteSpace(k.Type))
                .Select(k => (Type: k.Type.Trim(), k.Id))
                .Distinct()
                .ToList();

            var userIds = keys.Where(k => string.Equals(k.Type, "User", StringComparison.OrdinalIgnoreCase)).Select(k => k.Id).Distinct().ToList();
            var tenantIds = keys.Where(k => string.Equals(k.Type, "Tenant", StringComparison.OrdinalIgnoreCase)).Select(k => k.Id).Distinct().ToList();

            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Users (for User type and also tenant users when the id is the user id)
            if (userIds.Count > 0)
            {
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Name })
                    .ToListAsync();

                foreach (var u in users)
                {
                    lookup[NameKey("User", u.Id)] = u.Name;
                }
            }

            // Tenant type might actually store a tenant user id; first try Users table
            if (tenantIds.Count > 0)
            {
                var tenantUsers = await _context.Users
                    .Where(u => tenantIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Name })
                    .ToListAsync();

                foreach (var u in tenantUsers)
                {
                    var key = NameKey("Tenant", u.Id);
                    if (!lookup.ContainsKey(key))
                    {
                        lookup[key] = u.Name;
                    }
                }
            }

            // Then try tenant records by tenantId if still missing
            if (tenantIds.Count > 0)
            {
                var tenants = await _context.Tenants
                    .Where(t => tenantIds.Contains(t.TenantId))
                    .Select(t => new { Id = t.TenantId, Name = t.TenantName })
                    .ToListAsync();

                foreach (var t in tenants)
                {
                    var key = NameKey("Tenant", t.Id);
                    if (!lookup.ContainsKey(key))
                    {
                        lookup[key] = t.Name;
                    }
                }
            }

            return lookup;
        }

        private async Task<(int id, string type)> ResolveTenantReceiverAsync(int receiverId, string receiverType)
        {
            if (!string.Equals(receiverType, "Tenant", StringComparison.OrdinalIgnoreCase))
            {
                return (receiverId, receiverType);
            }

            // If the receiverId is a tenant record id and that tenant has a linked unit with TenantId (user id), use the user id
            var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == receiverId);
            if (tenant?.UnitId != null)
            {
                var unit = await _context.Units.AsNoTracking().FirstOrDefaultAsync(u => u.UnitId == tenant.UnitId.Value);
                if (unit?.TenantId != null && unit.TenantId.Value > 0)
                {
                    return (unit.TenantId.Value, "Tenant");
                }
            }

            return (receiverId, "Tenant");
        }

        private async Task<List<int>> GetTenantRecordIdsForUserAsync(int userId)
        {
            return await _context.Tenants
                .Where(t => t.UnitId != null)
                .Join(_context.Units.Where(u => u.TenantId == userId),
                    t => t.UnitId,
                    u => u.UnitId,
                    (t, u) => t.TenantId)
                .ToListAsync();
        }

        private string NameKey(string type, int id) => $"{type}:{id}";
    }
}

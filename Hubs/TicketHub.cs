using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace YopoBackend.Hubs
{
    public class TicketHub : Hub
    {
        public Task JoinUserGroup(int userId)
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue || currentUserId.Value != userId)
            {
                return Task.CompletedTask;
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        public Task LeaveUserGroup(int userId)
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue || currentUserId.Value != userId)
            {
                return Task.CompletedTask;
            }

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        public static string UserGroup(int userId) => $"User_{userId}";

        private int? GetUserId()
        {
            var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}

using Microsoft.AspNetCore.SignalR;

namespace YopoBackend.Hubs
{
    public class MessageHub : Hub
    {
        // Method for clients to join a specific group (e.g., their UserID or TenantID)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Method for clients to leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}

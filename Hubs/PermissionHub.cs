using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace YopoBackend.Hubs
{
    [Authorize]
    public class PermissionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
            }

            await base.OnConnectedAsync();
        }

        public static string GetUserGroupName(int userId) => $"permissions-user-{userId}";
    }
}

using Microsoft.AspNetCore.SignalR;

namespace YopoBackend.Hubs
{
    public class AnnouncementHub : Hub
    {
        public Task JoinBuildingGroup(int buildingId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(buildingId));
        }

        public Task LeaveBuildingGroup(int buildingId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(buildingId));
        }

        private static string GroupName(int buildingId) => $"Building_{buildingId}";
    }
}

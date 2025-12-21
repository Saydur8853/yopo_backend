using Microsoft.AspNetCore.SignalR;

namespace YopoBackend.Hubs
{
    public class ThreadSocialHub : Hub
    {
        public async Task JoinBuildingGroup(int buildingId)
        {
            if (buildingId <= 0)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(buildingId));
        }

        public async Task LeaveBuildingGroup(int buildingId)
        {
            if (buildingId <= 0)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(buildingId));
        }

        public static string GroupName(int buildingId) => $"ThreadBuilding_{buildingId}";
    }
}

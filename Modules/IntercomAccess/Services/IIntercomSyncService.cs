using YopoBackend.Modules.IntercomAccess.DTOs;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public interface IIntercomSyncService
    {
        Task<List<PendingTenantFaceDTO>> GetPendingTenantsAsync(int buildingId);
        Task<int> ConfirmSyncAsync(int buildingId);
        Task<(int inserted, int updated, int skipped)> BackfillPendingAsync(int? buildingId);
    }
}

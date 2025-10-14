using YopoBackend.Modules.TenantCRUD.DTOs;

namespace YopoBackend.Modules.TenantCRUD.Services
{
    public interface ITenantService
    {
        Task<(List<TenantResponseDTO> tenants, int totalRecords)> GetTenantsAsync(int currentUserId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? buildingId = null, int? floorId = null, int? unitId = null, bool? isActive = null, bool? isPaid = null);
        Task<TenantResponseDTO> CreateTenantAsync(CreateTenantDTO dto, int currentUserId);
        Task<TenantResponseDTO?> UpdateTenantAsync(int tenantId, UpdateTenantDTO dto, int currentUserId);
        Task<bool> ActivateTenantAsync(int tenantId, int currentUserId);
        Task<bool> DeactivateTenantAsync(int tenantId, int currentUserId);
        Task<bool> InviteTenantAsync(InviteTenantDTO dto, int currentUserId);
    }
}

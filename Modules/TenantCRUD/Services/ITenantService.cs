using YopoBackend.Modules.TenantCRUD.DTOs;

namespace YopoBackend.Modules.TenantCRUD.Services
{
    public interface ITenantService
    {
        Task<(List<TenantResponseDTO> tenants, int totalRecords)> GetTenantsAsync(int currentUserId, int page = 1, int pageSize = 10, string? searchTerm = null, int? buildingId = null, int? floorId = null, int? unitId = null, bool? isActive = null, bool? isPaid = null, int? tenantId = null);
        Task<TenantResponseDTO> CreateTenantAsync(CreateTenantDTO dto, int currentUserId);
        Task<TenantResponseDTO?> UpdateTenantAsync(int tenantId, UpdateTenantDTO dto, int currentUserId);
        Task<bool> UpdateTenantStatusAsync(int tenantId, UpdateTenantStatusDTO dto, int currentUserId);
        Task<bool> DeleteTenantAsync(int tenantId, int currentUserId);
    }
}

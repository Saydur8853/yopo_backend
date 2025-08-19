using YopoBackend.Modules.TenantCRUD.DTOs;

namespace YopoBackend.Modules.TenantCRUD.Services
{
    /// <summary>
    /// Interface for Tenant service operations.
    /// Module ID: 5 (TenantCRUD)
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Gets all tenants with pagination and optional search criteria.
        /// </summary>
        /// <param name="searchDto">Search criteria and pagination parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of tenant DTOs.</returns>
        Task<TenantListDto> GetAllTenantsAsync(TenantSearchDto searchDto);

        /// <summary>
        /// Gets all active tenants.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active tenant DTOs.</returns>
        Task<IEnumerable<TenantDto>> GetActiveTenantsAsync();

        /// <summary>
        /// Gets a tenant by its ID.
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the tenant DTO if found, otherwise null.</returns>
        Task<TenantDto?> GetTenantByIdAsync(int id);

        /// <summary>
        /// Gets tenants by building ID.
        /// </summary>
        /// <param name="buildingId">The building ID to filter by.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of tenant DTOs.</returns>
        Task<IEnumerable<TenantDto>> GetTenantsByBuildingIdAsync(int buildingId);

        /// <summary>
        /// Gets tenants by floor and building.
        /// </summary>
        /// <param name="buildingId">The building ID to filter by.</param>
        /// <param name="floor">The floor number to filter by.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of tenant DTOs.</returns>
        Task<IEnumerable<TenantDto>> GetTenantsByFloorAsync(int buildingId, int floor);

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="createTenantDto">The data for creating the tenant.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created tenant DTO.</returns>
        Task<TenantDto> CreateTenantAsync(CreateTenantDto createTenantDto);

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        /// <param name="id">The ID of the tenant to update.</param>
        /// <param name="updateTenantDto">The data for updating the tenant.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated tenant DTO if successful, otherwise null.</returns>
        Task<TenantDto?> UpdateTenantAsync(int id, UpdateTenantDto updateTenantDto);

        /// <summary>
        /// Deletes a tenant.
        /// </summary>
        /// <param name="id">The ID of the tenant to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, otherwise false.</returns>
        Task<bool> DeleteTenantAsync(int id);

        /// <summary>
        /// Checks if a tenant exists with the specified unit number in the same building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="unitNo">The unit number to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the unit exists, otherwise false.</returns>
        Task<bool> TenantExistsInUnitAsync(int buildingId, string unitNo, int? excludeId = null);

        /// <summary>
        /// Gets tenants with expiring contracts within the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look ahead for expiring contracts.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of tenants with expiring contracts.</returns>
        Task<IEnumerable<TenantDto>> GetTenantsWithExpiringContractsAsync(int days = 30);

        /// <summary>
        /// Gets tenants who haven't paid their dues.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of tenants who haven't paid.</returns>
        Task<IEnumerable<TenantDto>> GetUnpaidTenantsAsync();

        /// <summary>
        /// Updates the payment status for a tenant.
        /// </summary>
        /// <param name="id">The tenant ID.</param>
        /// <param name="paid">The payment status.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, otherwise false.</returns>
        Task<bool> UpdatePaymentStatusAsync(int id, bool paid);

        /// <summary>
        /// Gets tenant statistics for a building.
        /// </summary>
        /// <param name="buildingId">Optional building ID to filter by. If null, returns stats for all buildings.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains tenant statistics.</returns>
        Task<object> GetTenantStatisticsAsync(int? buildingId = null);
    }
}

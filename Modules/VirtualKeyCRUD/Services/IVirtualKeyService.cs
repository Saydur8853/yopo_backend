using YopoBackend.Modules.VirtualKeyCRUD.DTOs;

namespace YopoBackend.Modules.VirtualKeyCRUD.Services
{
    /// <summary>
    /// Interface for Virtual Key service operations.
    /// Module ID: 10 (VirtualKeyCRUD)
    /// </summary>
    public interface IVirtualKeyService
    {
        /// <summary>
        /// Gets all virtual keys based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all virtual keys the user has access to.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetAllVirtualKeysAsync(int userId);

        /// <summary>
        /// Gets all active virtual keys based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all active virtual keys the user has access to.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetActiveVirtualKeysAsync(int userId);

        /// <summary>
        /// Gets all expired virtual keys based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all expired virtual keys the user has access to.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetExpiredVirtualKeysAsync(int userId);

        /// <summary>
        /// Gets a virtual key by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The virtual key with the specified ID if the user has access to it, or null if not found or no access.</returns>
        Task<VirtualKeyDto?> GetVirtualKeyByIdAsync(int id, int userId);

        /// <summary>
        /// Gets all virtual keys for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys in the specified building.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByBuildingIdAsync(int buildingId, int userId);

        /// <summary>
        /// Gets all virtual keys assigned to a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys assigned to the specified tenant.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByTenantIdAsync(int tenantId, int userId);

        /// <summary>
        /// Gets all virtual keys associated with a specific intercom.
        /// </summary>
        /// <param name="intercomId">The ID of the intercom.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys associated with the specified intercom.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByIntercomIdAsync(int intercomId, int userId);

        /// <summary>
        /// Gets virtual keys by status.
        /// </summary>
        /// <param name="status">The status to filter by.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys with the specified status.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByStatusAsync(string status, int userId);

        /// <summary>
        /// Gets virtual keys by type.
        /// </summary>
        /// <param name="type">The type to filter by.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys with the specified type.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysByTypeAsync(string type, int userId);

        /// <summary>
        /// Creates a new virtual key.
        /// </summary>
        /// <param name="createVirtualKeyDto">The data for creating the virtual key.</param>
        /// <param name="createdByUserId">The ID of the user creating the virtual key.</param>
        /// <returns>The created virtual key.</returns>
        Task<VirtualKeyDto> CreateVirtualKeyAsync(CreateVirtualKeyDto createVirtualKeyDto, int createdByUserId);

        /// <summary>
        /// Updates an existing virtual key, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key to update.</param>
        /// <param name="updateVirtualKeyDto">The data for updating the virtual key.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated virtual key if the user has access to it, or null if the key was not found or no access.</returns>
        Task<VirtualKeyDto?> UpdateVirtualKeyAsync(int id, UpdateVirtualKeyDto updateVirtualKeyDto, int userId);

        /// <summary>
        /// Deletes a virtual key, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key to delete.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if the virtual key was deleted successfully and user has access, false if not found or no access.</returns>
        Task<bool> DeleteVirtualKeyAsync(int id, int userId);

        /// <summary>
        /// Records usage of a virtual key.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key being used.</param>
        /// <param name="usageLocation">The location where the key was used.</param>
        /// <param name="usageDetails">Additional details about the usage.</param>
        /// <returns>True if the usage was recorded successfully, false otherwise.</returns>
        Task<bool> RecordVirtualKeyUsageAsync(int keyId, string? usageLocation = null, string? usageDetails = null);

        /// <summary>
        /// Generates a unique PIN code for a virtual key.
        /// </summary>
        /// <returns>A unique PIN code string.</returns>
        Task<string> GenerateUniquePinCodeAsync();

        /// <summary>
        /// Generates a QR code for a virtual key.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key.</param>
        /// <param name="additionalData">Additional data to include in the QR code.</param>
        /// <returns>The QR code data string.</returns>
        Task<string> GenerateQrCodeAsync(int keyId, string? additionalData = null);

        /// <summary>
        /// Validates if a building exists and user has access to it.
        /// </summary>
        /// <param name="buildingId">The building ID to validate.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if building exists and user has access, false otherwise.</returns>
        Task<bool> ValidateBuildingAccessAsync(int buildingId, int userId);

        /// <summary>
        /// Validates if a tenant exists and user has access to it.
        /// </summary>
        /// <param name="tenantId">The tenant ID to validate.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if tenant exists and user has access, false otherwise.</returns>
        Task<bool> ValidateTenantAccessAsync(int tenantId, int userId);

        /// <summary>
        /// Validates if an intercom exists and user has access to it.
        /// </summary>
        /// <param name="intercomId">The intercom ID to validate.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if intercom exists and user has access, false otherwise.</returns>
        Task<bool> ValidateIntercomAccessAsync(int intercomId, int userId);

        /// <summary>
        /// Checks if a virtual key has expired.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key to check.</param>
        /// <returns>True if the virtual key has expired, false otherwise.</returns>
        Task<bool> IsVirtualKeyExpiredAsync(int keyId);

        /// <summary>
        /// Updates the status of expired virtual keys.
        /// </summary>
        /// <returns>The number of virtual keys updated.</returns>
        Task<int> UpdateExpiredVirtualKeysStatusAsync();

        /// <summary>
        /// Gets virtual keys that are about to expire within the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look ahead for expiring keys.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of virtual keys that will expire soon.</returns>
        Task<IEnumerable<VirtualKeyDto>> GetVirtualKeysExpiringInDaysAsync(int days, int userId);
    }
}

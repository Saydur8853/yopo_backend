using YopoBackend.Modules.CCTVcrud.DTOs;

namespace YopoBackend.Modules.CCTVcrud.Services
{
    /// <summary>
    /// Interface for CCTV service operations.
    /// Module ID: 8 (CCTVcrud)
    /// </summary>
    public interface ICCTVService
    {
        /// <summary>
        /// Gets all CCTV cameras based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all CCTV cameras the user has access to.</returns>
        Task<IEnumerable<CCTVDto>> GetAllCCTVsAsync(int userId);

        /// <summary>
        /// Gets all active CCTV cameras based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all active CCTV cameras the user has access to.</returns>
        Task<IEnumerable<CCTVDto>> GetActiveCCTVsAsync(int userId);

        /// <summary>
        /// Gets all public CCTV cameras (accessible to all users).
        /// </summary>
        /// <returns>A list of all public CCTV cameras.</returns>
        Task<IEnumerable<CCTVSummaryDto>> GetPublicCCTVsAsync();

        /// <summary>
        /// Gets a CCTV camera by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The CCTV camera with the specified ID if the user has access to it, or null if not found or no access.</returns>
        Task<CCTVDto?> GetCCTVByIdAsync(int id, int userId);

        /// <summary>
        /// Gets all CCTV cameras for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of CCTV cameras in the specified building.</returns>
        Task<IEnumerable<CCTVDto>> GetCCTVsByBuildingIdAsync(int buildingId, int userId);

        /// <summary>
        /// Gets all CCTV cameras assigned to a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of CCTV cameras assigned to the specified tenant.</returns>
        Task<IEnumerable<CCTVDto>> GetCCTVsByTenantIdAsync(int tenantId, int userId);

        /// <summary>
        /// Creates a new CCTV camera.
        /// </summary>
        /// <param name="createCCTVDto">The data for creating the CCTV camera.</param>
        /// <param name="createdByUserId">The ID of the user creating the CCTV camera.</param>
        /// <returns>The created CCTV camera.</returns>
        Task<CCTVDto> CreateCCTVAsync(CreateCCTVDto createCCTVDto, int createdByUserId);

        /// <summary>
        /// Updates an existing CCTV camera, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera to update.</param>
        /// <param name="updateCCTVDto">The data for updating the CCTV camera.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated CCTV camera if the user has access to it, or null if the camera was not found or no access.</returns>
        Task<CCTVDto?> UpdateCCTVAsync(int id, UpdateCCTVDto updateCCTVDto, int userId);

        /// <summary>
        /// Deletes a CCTV camera, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the CCTV camera to delete.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if the CCTV camera was deleted successfully and user has access, false if not found or no access.</returns>
        Task<bool> DeleteCCTVAsync(int id, int userId);

        /// <summary>
        /// Checks if a CCTV camera name already exists in the same building.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="buildingId">The building ID to check within.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists in the building, false otherwise.</returns>
        Task<bool> CCTVExistsInBuildingAsync(string name, int buildingId, int? excludeId = null);

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
        /// Gets CCTV cameras with stream URLs for monitoring dashboard.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="buildingId">Optional building ID to filter by.</param>
        /// <returns>A list of CCTV cameras with stream information.</returns>
        Task<IEnumerable<CCTVDto>> GetCCTVsForMonitoringAsync(int userId, int? buildingId = null);
    }
}

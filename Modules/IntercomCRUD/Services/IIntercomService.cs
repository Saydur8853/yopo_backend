using YopoBackend.Modules.IntercomCRUD.DTOs;

namespace YopoBackend.Modules.IntercomCRUD.Services
{
    /// <summary>
    /// Interface for Intercom service operations.
    /// Module ID: 9 (IntercomCRUD)
    /// </summary>
    public interface IIntercomService
    {
        /// <summary>
        /// Gets all intercom systems based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all intercom systems the user has access to.</returns>
        Task<IEnumerable<IntercomListDto>> GetAllIntercomsAsync(int userId);

        /// <summary>
        /// Gets all active intercom systems based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all active intercom systems the user has access to.</returns>
        Task<IEnumerable<IntercomListDto>> GetActiveIntercomsAsync(int userId);

        /// <summary>
        /// Gets all installed intercom systems based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all installed intercom systems the user has access to.</returns>
        Task<IEnumerable<IntercomListDto>> GetInstalledIntercomsAsync(int userId);

        /// <summary>
        /// Gets an intercom system by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The intercom system with the specified ID if the user has access to it, or null if not found or no access.</returns>
        Task<IntercomDto?> GetIntercomByIdAsync(int id, int userId);

        /// <summary>
        /// Gets all intercom systems for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of intercom systems in the specified building.</returns>
        Task<IEnumerable<IntercomListDto>> GetIntercomsByBuildingIdAsync(int buildingId, int userId);

        /// <summary>
        /// Gets all intercom systems that have CCTV integration.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of intercom systems with CCTV integration.</returns>
        Task<IEnumerable<IntercomListDto>> GetIntercomsWithCCTVAsync(int userId);

        /// <summary>
        /// Gets all intercom systems that have PIN pad functionality.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of intercom systems with PIN pad functionality.</returns>
        Task<IEnumerable<IntercomListDto>> GetIntercomsWithPinPadAsync(int userId);

        /// <summary>
        /// Creates a new intercom system.
        /// </summary>
        /// <param name="createIntercomDto">The data for creating the intercom system.</param>
        /// <param name="createdByUserId">The ID of the user creating the intercom system.</param>
        /// <returns>The created intercom system.</returns>
        Task<IntercomDto> CreateIntercomAsync(CreateIntercomDto createIntercomDto, int createdByUserId);

        /// <summary>
        /// Updates an existing intercom system, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the intercom system to update.</param>
        /// <param name="updateIntercomDto">The data for updating the intercom system.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated intercom system if the user has access to it, or null if the system was not found or no access.</returns>
        Task<IntercomDto?> UpdateIntercomAsync(int id, UpdateIntercomDto updateIntercomDto, int userId);

        /// <summary>
        /// Deletes an intercom system, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the intercom system to delete.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if the intercom system was deleted successfully and user has access, false if not found or no access.</returns>
        Task<bool> DeleteIntercomAsync(int id, int userId);

        /// <summary>
        /// Checks if an intercom system name already exists in the same building.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="buildingId">The building ID to check within.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists in the building, false otherwise.</returns>
        Task<bool> IntercomExistsInBuildingAsync(string name, int buildingId, int? excludeId = null);

        /// <summary>
        /// Validates if a building exists and user has access to it.
        /// </summary>
        /// <param name="buildingId">The building ID to validate.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if building exists and user has access, false otherwise.</returns>
        Task<bool> ValidateBuildingAccessAsync(int buildingId, int userId);

        /// <summary>
        /// Gets intercom systems that require maintenance (based on service date).
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="monthsThreshold">Number of months since last service to consider requiring maintenance (default: 12).</param>
        /// <returns>A list of intercom systems that may require maintenance.</returns>
        Task<IEnumerable<IntercomListDto>> GetIntercomsRequiringMaintenanceAsync(int userId, int monthsThreshold = 12);

        /// <summary>
        /// Gets intercom systems with warranty expiring soon.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="daysThreshold">Number of days ahead to check for warranty expiry (default: 90).</param>
        /// <returns>A list of intercom systems with warranty expiring soon.</returns>
        Task<IEnumerable<IntercomListDto>> GetIntercomsWithExpiringWarrantyAsync(int userId, int daysThreshold = 90);

        /// <summary>
        /// Toggles the active status of an intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated intercom system if successful, null if not found or no access.</returns>
        Task<IntercomDto?> ToggleIntercomStatusAsync(int id, int userId);

        /// <summary>
        /// Updates the service date of an intercom system.
        /// </summary>
        /// <param name="id">The ID of the intercom system.</param>
        /// <param name="serviceDate">The new service date.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated intercom system if successful, null if not found or no access.</returns>
        Task<IntercomDto?> UpdateServiceDateAsync(int id, DateTime serviceDate, int userId);
    }
}

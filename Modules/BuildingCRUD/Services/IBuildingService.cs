using YopoBackend.Modules.BuildingCRUD.DTOs;

namespace YopoBackend.Modules.BuildingCRUD.Services
{
    /// <summary>
    /// Interface for building service operations.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>
        /// Gets all buildings based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all buildings the user has access to.</returns>
        Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync(int userId);

        /// <summary>
        /// Gets all active buildings based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all active buildings the user has access to.</returns>
        Task<IEnumerable<BuildingDto>> GetActiveBuildingsAsync(int userId);

        /// <summary>
        /// Gets a building by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The building with the specified ID if the user has access to it, or null if not found or no access.</returns>
        Task<BuildingDto?> GetBuildingByIdAsync(int id, int userId);

        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="createBuildingDto">The data for creating the building.</param>
        /// <param name="createdByUserId">The ID of the user creating the building.</param>
        /// <returns>The created building.</returns>
        Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto, int createdByUserId);

        /// <summary>
        /// Updates an existing building, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building to update.</param>
        /// <param name="updateBuildingDto">The data for updating the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated building if the user has access to it, or null if the building was not found or no access.</returns>
        Task<BuildingDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto, int userId);

        /// <summary>
        /// Deletes a building, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the building to delete.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if the building was deleted successfully and user has access, false if not found or no access.</returns>
        Task<bool> DeleteBuildingAsync(int id, int userId);

        /// <summary>
        /// Checks if a building name already exists.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if the name exists, false otherwise.</returns>
        Task<bool> BuildingExistsAsync(string name, int? excludeId = null);

        /// <summary>
        /// Initializes sample buildings for demonstration purposes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InitializeSampleBuildingsAsync();
    }
}

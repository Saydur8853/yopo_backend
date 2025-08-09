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
        /// Gets all buildings.
        /// </summary>
        /// <returns>A list of all buildings.</returns>
        Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync();

        /// <summary>
        /// Gets all active buildings.
        /// </summary>
        /// <returns>A list of all active buildings.</returns>
        Task<IEnumerable<BuildingDto>> GetActiveBuildingsAsync();

        /// <summary>
        /// Gets a building by ID.
        /// </summary>
        /// <param name="id">The ID of the building.</param>
        /// <returns>The building with the specified ID, or null if not found.</returns>
        Task<BuildingDto?> GetBuildingByIdAsync(int id);

        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="createBuildingDto">The data for creating the building.</param>
        /// <returns>The created building.</returns>
        Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto);

        /// <summary>
        /// Updates an existing building.
        /// </summary>
        /// <param name="id">The ID of the building to update.</param>
        /// <param name="updateBuildingDto">The data for updating the building.</param>
        /// <returns>The updated building, or null if the building was not found.</returns>
        Task<BuildingDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto);

        /// <summary>
        /// Deletes a building.
        /// </summary>
        /// <param name="id">The ID of the building to delete.</param>
        /// <returns>True if the building was deleted successfully, false if not found.</returns>
        Task<bool> DeleteBuildingAsync(int id);

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

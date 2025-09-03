using YopoBackend.Modules.DoorCRUD.DTOs;

namespace YopoBackend.Modules.DoorCRUD.Services
{
    /// <summary>
    /// Interface for door service operations.
    /// Module ID: 12 (DoorCRUD)
    /// </summary>
    public interface IDoorService
    {
        /// <summary>
        /// Gets all doors based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all doors the user has access to.</returns>
        Task<IEnumerable<DoorDto>> GetAllDoorsAsync(int userId);

        /// <summary>
        /// Gets all active doors based on user's access control.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of all active doors the user has access to.</returns>
        Task<IEnumerable<DoorDto>> GetActiveDoorsAsync(int userId);

        /// <summary>
        /// Gets doors by building ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A list of doors for the specified building if the user has access.</returns>
        Task<IEnumerable<DoorDto>> GetDoorsByBuildingIdAsync(int buildingId, int userId);

        /// <summary>
        /// Gets a door by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The door with the specified ID if the user has access to it, or null if not found or no access.</returns>
        Task<DoorDto?> GetDoorByIdAsync(int id, int userId);

        /// <summary>
        /// Creates a new door.
        /// </summary>
        /// <param name="createDoorDto">The data for creating the door.</param>
        /// <param name="createdByUserId">The ID of the user creating the door.</param>
        /// <returns>The created door.</returns>
        Task<DoorDto> CreateDoorAsync(CreateDoorDto createDoorDto, int createdByUserId);

        /// <summary>
        /// Updates an existing door, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door to update.</param>
        /// <param name="updateDoorDto">The data for updating the door.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>The updated door if the user has access to it, or null if the door was not found or no access.</returns>
        Task<DoorDto?> UpdateDoorAsync(int id, UpdateDoorDto updateDoorDto, int userId);

        /// <summary>
        /// Deletes a door, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the door to delete.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>True if the door was deleted successfully and user has access, false if not found or no access.</returns>
        Task<bool> DeleteDoorAsync(int id, int userId);

        /// <summary>
        /// Checks if a door with the same type and location already exists in a building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="type">The door type.</param>
        /// <param name="location">The door location.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
        /// <returns>True if a similar door exists, false otherwise.</returns>
        Task<bool> DoorExistsAsync(int buildingId, string type, string? location = null, int? excludeId = null);
    }
}

using YopoBackend.Modules.BuildingCRUD.DTOs;

namespace YopoBackend.Modules.BuildingCRUD.Services
{
    /// <summary>
    /// Interface for Building service operations with PM data access control.
    /// Module ID: 4 (BuildingCRUD)
    /// Data Access Control: PM (Super Admin sees all, Property Manager sees own customer buildings)
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>
        /// Gets all buildings with pagination and filtering based on user's data access control.
        /// </summary>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <param name="page">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="searchTerm">Optional search term for building name or address.</param>
        /// <param name="customerId">Optional filter by customer ID.</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <param name="hasGym">Optional filter by gym amenity.</param>
        /// <param name="hasSwimmingPool">Optional filter by swimming pool amenity.</param>
        /// <param name="hasSauna">Optional filter by sauna amenity.</param>
        /// <returns>Paginated list of buildings.</returns>
        Task<BuildingListResponseDTO> GetBuildingsAsync(
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            int? customerId = null,
            bool? isActive = null,
            bool? hasGym = null,
            bool? hasSwimmingPool = null,
            bool? hasSauna = null);

        /// <summary>
        /// Gets a building by ID with access control validation.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <returns>The building if found and accessible, null otherwise.</returns>
        Task<BuildingResponseDTO?> GetBuildingByIdAsync(int buildingId, int currentUserId);

        /// <summary>
        /// Creates a new building.
        /// </summary>
        /// <param name="createBuildingDto">The building creation data.</param>
        /// <param name="currentUserId">The current user's ID.</param>
        /// <returns>The created building.</returns>
        Task<BuildingResponseDTO> CreateBuildingAsync(CreateBuildingDTO createBuildingDto, int currentUserId);

        /// <summary>
        /// Updates an existing building with access control validation.
        /// </summary>
        /// <param name="buildingId">The building ID to update.</param>
        /// <param name="updateBuildingDto">The update data.</param>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <returns>The updated building if successful, null if not found or no access.</returns>
        Task<BuildingResponseDTO?> UpdateBuildingAsync(int buildingId, UpdateBuildingDTO updateBuildingDto, int currentUserId);

        /// <summary>
        /// Deletes a building with access control validation.
        /// </summary>
        /// <param name="buildingId">The building ID to delete.</param>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <returns>True if deleted successfully, false if not found or no access.</returns>
        Task<bool> DeleteBuildingAsync(int buildingId, int currentUserId);

        /// <summary>
        /// Gets building amenities summary for a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <returns>Building amenities information if accessible, null otherwise.</returns>
        Task<BuildingAmenitiesDTO?> GetBuildingAmenitiesAsync(int buildingId, int currentUserId);

        /// <summary>
        /// Gets buildings for a specific customer with access control validation.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="currentUserId">The current user's ID for access control.</param>
        /// <param name="page">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <returns>Paginated list of buildings for the customer if accessible.</returns>
        Task<BuildingListResponseDTO> GetBuildingsByCustomerAsync(
            int customerId,
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            bool? isActive = null);

        /// <summary>
        /// Validates if a customer ID is accessible by the current user based on PM access control.
        /// </summary>
        /// <param name="customerId">The customer ID to validate.</param>
        /// <param name="currentUserId">The current user's ID.</param>
        /// <returns>True if the customer is accessible, false otherwise.</returns>
        Task<bool> ValidateCustomerAccessAsync(int customerId, int currentUserId);
    }
}
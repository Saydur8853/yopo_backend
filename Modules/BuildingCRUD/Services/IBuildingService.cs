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
        /// <param name="buildingId">Optional filter by specific building ID.</param>
        /// <param name="isActive">Optional filter by active status.</param>
        /// <returns>Paginated list of buildings.</returns>
        Task<BuildingListResponseDTO> GetBuildingsAsync(
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            int? customerId = null,
            int? buildingId = null,
            bool? isActive = null);


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
        /// Validates if a customer ID is accessible by the current user based on PM access control.
        /// </summary>
        /// <param name="customerId">The customer ID to validate.</param>
        /// <param name="currentUserId">The current user's ID.</param>
        /// <returns>True if the customer is accessible, false otherwise.</returns>
        Task<bool> ValidateCustomerAccessAsync(int customerId, int currentUserId);
    }
}
using YopoBackend.Modules.AmenityCRUD.DTOs;

namespace YopoBackend.Modules.AmenityCRUD.Services
{
    /// <summary>
    /// Interface for Amenity service operations.
    /// </summary>
    public interface IAmenityService
    {
        /// <summary>
        /// Retrieves all amenities belonging to a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID to filter amenities by.</param>
        /// <returns>Service result with list of amenities.</returns>
        Task<(bool Success, string Message, List<AmenityResponseDTO>? Data)> GetAmenitiesByBuildingAsync(int buildingId);

        /// <summary>
        /// Creates a new amenity under a building.
        /// </summary>
        /// <param name="dto">The amenity creation data.</param>
        /// <returns>Service result with created amenity data.</returns>
        Task<(bool Success, string Message, AmenityResponseDTO? Data)> CreateAmenityAsync(CreateAmenityDTO dto);

        /// <summary>
        /// Updates an existing amenity by its ID.
        /// </summary>
        /// <param name="amenityId">The amenity ID to update.</param>
        /// <param name="dto">The update data.</param>
        /// <returns>Service result with updated amenity data.</returns>
        Task<(bool Success, string Message, AmenityResponseDTO? Data)> UpdateAmenityAsync(int amenityId, UpdateAmenityDTO dto);

        /// <summary>
        /// Deletes an amenity by its ID.
        /// </summary>
        /// <param name="amenityId">The amenity ID to delete.</param>
        /// <returns>Service result indicating success or failure.</returns>
        Task<(bool Success, string Message)> DeleteAmenityAsync(int amenityId);
    }
}
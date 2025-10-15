using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.AmenityCRUD.DTOs;
using YopoBackend.Modules.AmenityCRUD.Models;

namespace YopoBackend.Modules.AmenityCRUD.Services
{
    /// <summary>
    /// Service for managing amenity operations.
    /// </summary>
    public class AmenityService : IAmenityService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the AmenityService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public AmenityService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<(List<AmenityResponseDTO> amenities, int totalRecords)> GetAmenitiesByBuildingAsync(int buildingId, int page, int pageSize)
        {
            var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == buildingId);
            if (!buildingExists)
            {
                return (new List<AmenityResponseDTO>(), 0);
            }

            var query = _context.Amenities
                .Where(a => a.BuildingId == buildingId)
                .OrderBy(a => a.Name)
                .AsNoTracking();

            var totalRecords = await query.CountAsync();

            var amenities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AmenityResponseDTO
                {
                    AmenityId = a.AmenityId,
                    BuildingId = a.BuildingId,
                    Name = a.Name,
                    Type = a.Type,
                    Description = a.Description,
                    FloorId = a.FloorId,
                    IsAvailable = a.IsAvailable,
                    OpenHours = a.OpenHours,
                    AccessControl = a.AccessControl,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return (amenities, totalRecords);
        }

        /// <inheritdoc/>
        public async Task<(bool Success, string Message, AmenityResponseDTO? Data)> CreateAmenityAsync(CreateAmenityDTO dto)
        {
            // Validate building exists
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId);
            if (building == null)
            {
                return (false, $"Building with ID {dto.BuildingId} not found.", null);
            }

            // Validate floor exists if provided and belongs to the building
            if (dto.FloorId.HasValue)
            {
                var floor = await _context.Floors.FirstOrDefaultAsync(f => f.FloorId == dto.FloorId.Value);
                if (floor == null)
                {
                    return (false, $"Floor with ID {dto.FloorId.Value} not found.", null);
                }
                if (floor.BuildingId != dto.BuildingId)
                {
                    return (false, "The specified floor does not belong to the specified building.", null);
                }
            }

            var amenity = new Amenity
            {
                BuildingId = dto.BuildingId,
                Name = dto.Name,
                Type = dto.Type,
                Description = dto.Description,
                FloorId = dto.FloorId,
                IsAvailable = dto.IsAvailable,
                OpenHours = dto.OpenHours,
                AccessControl = dto.AccessControl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            var response = new AmenityResponseDTO
            {
                AmenityId = amenity.AmenityId,
                BuildingId = amenity.BuildingId,
                Name = amenity.Name,
                Type = amenity.Type,
                Description = amenity.Description,
                FloorId = amenity.FloorId,
                IsAvailable = amenity.IsAvailable,
                OpenHours = amenity.OpenHours,
                AccessControl = amenity.AccessControl,
                CreatedAt = amenity.CreatedAt,
                UpdatedAt = amenity.UpdatedAt
            };

            return (true, "Amenity created successfully.", response);
        }

        /// <inheritdoc/>
        public async Task<(bool Success, string Message, AmenityResponseDTO? Data)> UpdateAmenityAsync(int amenityId, UpdateAmenityDTO dto)
        {
            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.AmenityId == amenityId);
            if (amenity == null)
            {
                return (false, $"Amenity with ID {amenityId} not found.", null);
            }

            // Validate floor exists if provided and belongs to the same building
            if (dto.FloorId.HasValue)
            {
                var floor = await _context.Floors.FirstOrDefaultAsync(f => f.FloorId == dto.FloorId.Value);
                if (floor == null)
                {
                    return (false, $"Floor with ID {dto.FloorId.Value} not found.", null);
                }
                if (floor.BuildingId != amenity.BuildingId)
                {
                    return (false, "The specified floor does not belong to the amenity's building.", null);
                }
            }

            // Update fields if provided
            if (dto.Name != null) amenity.Name = dto.Name;
            if (dto.Type != null) amenity.Type = dto.Type;
            if (dto.Description != null) amenity.Description = dto.Description;
            if (dto.FloorId.HasValue) amenity.FloorId = dto.FloorId.Value;
            if (dto.IsAvailable.HasValue) amenity.IsAvailable = dto.IsAvailable.Value;
            if (dto.OpenHours != null) amenity.OpenHours = dto.OpenHours;
            if (dto.AccessControl != null) amenity.AccessControl = dto.AccessControl;

            amenity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new AmenityResponseDTO
            {
                AmenityId = amenity.AmenityId,
                BuildingId = amenity.BuildingId,
                Name = amenity.Name,
                Type = amenity.Type,
                Description = amenity.Description,
                FloorId = amenity.FloorId,
                IsAvailable = amenity.IsAvailable,
                OpenHours = amenity.OpenHours,
                AccessControl = amenity.AccessControl,
                CreatedAt = amenity.CreatedAt,
                UpdatedAt = amenity.UpdatedAt
            };

            return (true, "Amenity updated successfully.", response);
        }

        /// <inheritdoc/>
        public async Task<(bool Success, string Message)> DeleteAmenityAsync(int amenityId)
        {
            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.AmenityId == amenityId);
            if (amenity == null)
            {
                return (false, $"Amenity with ID {amenityId} not found.");
            }

            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();

            return (true, "Amenity deleted successfully.");
        }
    }
}
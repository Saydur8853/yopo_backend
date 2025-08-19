using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.BuildingCRUD.Services
{
    /// <summary>
    /// Service implementation for Building operations.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class BuildingService : BaseAccessControlService, IBuildingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public BuildingService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync(int userId)
        {
            var buildings = await GetBuildingsBasedOnUserAccess(userId, includeInactive: true);
            return buildings.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BuildingDto>> GetActiveBuildingsAsync(int userId)
        {
            var buildings = await GetBuildingsBasedOnUserAccess(userId, includeInactive: false);
            return buildings.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<BuildingDto?> GetBuildingByIdAsync(int id, int userId)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == id);
            if (building == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(building, userId))
            {
                return null; // User doesn't have access to this building
            }

            return MapToDto(building);
        }

        /// <inheritdoc/>
        public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto, int createdByUserId)
        {
            var building = new Building
            {
                Name = createBuildingDto.Name,
                Address = createBuildingDto.Address,
                Photo = createBuildingDto.Photo,
                Type = createBuildingDto.Type,
                Floors = createBuildingDto.Floors,
                ParkingFloor = createBuildingDto.ParkingFloor,
                ParkingSpace = createBuildingDto.ParkingSpace,
                Units = createBuildingDto.Units,
                CommercialUnit = createBuildingDto.CommercialUnit,
                HasGym = createBuildingDto.HasGym,
                HasSwimpool = createBuildingDto.HasSwimpool,
                HasSauna = createBuildingDto.HasSauna,
                HasReception = createBuildingDto.HasReception,
                Developer = createBuildingDto.Developer,
                Color = createBuildingDto.Color,
                DateStartOperation = createBuildingDto.DateStartOperation,
                IsActive = true,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return MapToDto(building);
        }

        /// <inheritdoc/>
        public async Task<BuildingDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto, int userId)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == id);
            if (building == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(building, userId))
            {
                return null; // User doesn't have access to update this building
            }

            building.Name = updateBuildingDto.Name;
            building.Address = updateBuildingDto.Address;
            building.Photo = updateBuildingDto.Photo;
            building.Type = updateBuildingDto.Type;
            building.Floors = updateBuildingDto.Floors;
            building.ParkingFloor = updateBuildingDto.ParkingFloor;
            building.ParkingSpace = updateBuildingDto.ParkingSpace;
            building.Units = updateBuildingDto.Units;
            building.CommercialUnit = updateBuildingDto.CommercialUnit;
            building.HasGym = updateBuildingDto.HasGym;
            building.HasSwimpool = updateBuildingDto.HasSwimpool;
            building.HasSauna = updateBuildingDto.HasSauna;
            building.HasReception = updateBuildingDto.HasReception;
            building.Developer = updateBuildingDto.Developer;
            building.Color = updateBuildingDto.Color;
            building.DateStartOperation = updateBuildingDto.DateStartOperation;
            building.IsActive = updateBuildingDto.IsActive;
            building.UpdatedAt = DateTime.UtcNow;

            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();

            return MapToDto(building);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteBuildingAsync(int id, int userId)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == id);
            if (building == null)
            {
                return false;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(building, userId))
            {
                return false; // User doesn't have access to delete this building
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> BuildingExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Buildings.Where(b => b.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc/>
        public async Task InitializeSampleBuildingsAsync()
        {
            // DISABLED: Auto-insertion of dummy building data has been disabled
            // The buildings table should remain empty until actual data is added via the API
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets buildings based on user's access control settings.
        /// </summary>
        /// <param name="userId">The ID of the user requesting access.</param>
        /// <param name="includeInactive">Whether to include inactive buildings.</param>
        /// <returns>List of buildings the user has access to.</returns>
        private async Task<List<Building>> GetBuildingsBasedOnUserAccess(int userId, bool includeInactive)
        {
            var query = _context.Buildings.AsQueryable();

            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);

            // Apply active/inactive filter
            if (!includeInactive)
            {
                query = query.Where(b => b.IsActive);
            }

            return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Maps a Building entity to a BuildingDto.
        /// </summary>
        /// <param name="building">The building entity to map.</param>
        /// <returns>The mapped building DTO.</returns>
        private static BuildingDto MapToDto(Building building)
        {
            return new BuildingDto
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                Photo = building.Photo,
                Type = building.Type,
                Floors = building.Floors,
                ParkingFloor = building.ParkingFloor,
                ParkingSpace = building.ParkingSpace,
                Units = building.Units,
                CommercialUnit = building.CommercialUnit,
                HasGym = building.HasGym,
                HasSwimpool = building.HasSwimpool,
                HasSauna = building.HasSauna,
                HasReception = building.HasReception,
                Developer = building.Developer,
                Color = building.Color,
                DateStartOperation = building.DateStartOperation,
                IsActive = building.IsActive,
                CreatedBy = building.CreatedBy,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };
        }
    }
}

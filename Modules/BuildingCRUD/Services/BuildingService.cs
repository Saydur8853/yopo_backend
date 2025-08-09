using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Models;

namespace YopoBackend.Modules.BuildingCRUD.Services
{
    /// <summary>
    /// Service implementation for Building operations.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class BuildingService : IBuildingService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public BuildingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync()
        {
            var buildings = await _context.Buildings.ToListAsync();
            return buildings.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BuildingDto>> GetActiveBuildingsAsync()
        {
            var buildings = await _context.Buildings
                .Where(b => b.IsActive)
                .ToListAsync();
            return buildings.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<BuildingDto?> GetBuildingByIdAsync(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            return building != null ? MapToDto(building) : null;
        }

        /// <inheritdoc/>
        public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto createBuildingDto)
        {
            var building = new Building
            {
                Name = createBuildingDto.Name,
                Address = createBuildingDto.Address,
                Photo = createBuildingDto.Photo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return MapToDto(building);
        }

        /// <inheritdoc/>
        public async Task<BuildingDto?> UpdateBuildingAsync(int id, UpdateBuildingDto updateBuildingDto)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return null;
            }

            building.Name = updateBuildingDto.Name;
            building.Address = updateBuildingDto.Address;
            building.Photo = updateBuildingDto.Photo;
            building.IsActive = updateBuildingDto.IsActive;
            building.UpdatedAt = DateTime.UtcNow;

            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();

            return MapToDto(building);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteBuildingAsync(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return false;
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
                IsActive = building.IsActive,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.FloorCRUD.DTOs;
using YopoBackend.Modules.FloorCRUD.Models;

namespace YopoBackend.Modules.FloorCRUD.Services
{
    /// <summary>
    /// Service handling floor CRUD operations.
    /// </summary>
    public class FloorService : IFloorService
    {
        private readonly ApplicationDbContext _context;

        public FloorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<FloorResponseDTO> floors, int totalRecords)> GetFloorsAsync(int? buildingId, int? userId, int pageNumber, int pageSize)
        {
            var query = _context.Floors.AsQueryable();

            if (buildingId.HasValue)
            {
                var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == buildingId.Value);
                if (!buildingExists)
                {
                    return (new List<FloorResponseDTO>(), 0);
                }
                query = query.Where(f => f.BuildingId == buildingId.Value);
            }
            else if (userId.HasValue)
            {
                var buildingIds = await _context.Buildings
                    .Where(b => b.CreatedBy == userId.Value)
                    .Select(b => b.BuildingId)
                    .ToListAsync();

                if (!buildingIds.Any())
                {
                    return (new List<FloorResponseDTO>(), 0);
                }

                query = query.Where(f => buildingIds.Contains(f.BuildingId));
            }
            else
            {
                // If no buildingId and no userId, return empty list
                return (new List<FloorResponseDTO>(), 0);
            }

            var totalRecords = await query.CountAsync();

            var floors = await query
                .OrderBy(f => f.Number)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FloorResponseDTO
                {
                    FloorId = f.FloorId,
                    BuildingId = f.BuildingId,
                    Name = f.Name,
                    Number = f.Number,
                    Type = f.Type,
                    TotalUnits = f.TotalUnits,
                    AreaSqFt = f.AreaSqFt,
                    IsActive = f.IsActive,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();

            return (floors, totalRecords);
        }

        public async Task<FloorResponseDTO?> CreateFloorAsync(CreateFloorDTO dto)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId);
            if (building == null)
            {
                return null; // Building must exist
            }

            var floor = new Floor
            {
                BuildingId = dto.BuildingId,
                Name = dto.Name,
                Number = dto.Number,
                Type = dto.Type,
                TotalUnits = dto.TotalUnits,
                AreaSqFt = dto.AreaSqFt,
                IsActive = dto.IsActive,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();

            return new FloorResponseDTO
            {
                FloorId = floor.FloorId,
                BuildingId = floor.BuildingId,
                Name = floor.Name,
                Number = floor.Number,
                Type = floor.Type,
                TotalUnits = floor.TotalUnits,
                AreaSqFt = floor.AreaSqFt,
                IsActive = floor.IsActive,
                Status = floor.Status,
                CreatedAt = floor.CreatedAt,
                UpdatedAt = floor.UpdatedAt
            };
        }

        public async Task<FloorResponseDTO?> UpdateFloorAsync(int floorId, UpdateFloorDTO dto)
        {
            var floor = await _context.Floors.FirstOrDefaultAsync(f => f.FloorId == floorId);
            if (floor == null)
                return null;

            if (dto.Name != null) floor.Name = dto.Name;
            if (dto.Number.HasValue) floor.Number = dto.Number.Value;
            if (dto.Type != null) floor.Type = dto.Type;
            if (dto.TotalUnits.HasValue) floor.TotalUnits = dto.TotalUnits.Value;
            if (dto.AreaSqFt.HasValue) floor.AreaSqFt = dto.AreaSqFt.Value;
            if (dto.IsActive.HasValue) floor.IsActive = dto.IsActive.Value;
            if (dto.Status != null) floor.Status = dto.Status;

            floor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FloorResponseDTO
            {
                FloorId = floor.FloorId,
                BuildingId = floor.BuildingId,
                Name = floor.Name,
                Number = floor.Number,
                Type = floor.Type,
                TotalUnits = floor.TotalUnits,
                AreaSqFt = floor.AreaSqFt,
                IsActive = floor.IsActive,
                Status = floor.Status,
                CreatedAt = floor.CreatedAt,
                UpdatedAt = floor.UpdatedAt
            };
        }

        public async Task<(bool success, string floorName, string buildingName)> DeleteFloorAsync(int floorId)
        {
            var floor = await _context.Floors.Include(f => f.Building).FirstOrDefaultAsync(f => f.FloorId == floorId);
            if (floor == null)
                return (false, null, null);

            var floorName = floor.Name;
            var buildingName = floor.Building.Name;

            _context.Floors.Remove(floor);
            await _context.SaveChangesAsync();
            return (true, floorName, buildingName);
        }
    }
}
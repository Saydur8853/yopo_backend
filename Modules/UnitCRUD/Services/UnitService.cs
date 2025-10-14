using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YopoBackend.Data;
using YopoBackend.Modules.UnitCRUD.DTOs;
using YopoBackend.Modules.UnitCRUD.Models;

namespace YopoBackend.Modules.UnitCRUD.Services
{
    public class UnitService : IUnitService
    {
        private readonly ApplicationDbContext _context;
        public UnitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<UnitResponseDTO> units, int totalRecords)> GetUnitsByFloorAsync(int floorId, int pageNumber, int pageSize)
        {
            var floorExists = await _context.Floors.AnyAsync(f => f.FloorId == floorId);
            if (!floorExists)
            {
                return (new List<UnitResponseDTO>(), 0);
            }

            var query = _context.Units
                .Where(u => u.FloorId == floorId)
                .OrderBy(u => u.UnitNumber)
                .AsNoTracking();

            var totalRecords = await query.CountAsync();

            var unitEntities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var units = unitEntities.Select(u => new UnitResponseDTO
            {
                UnitId = u.UnitId,
                FloorId = u.FloorId,
                BuildingId = u.BuildingId,
                UnitNumber = u.UnitNumber,
                Type = u.Type,
                Category = u.Category,
                AreaSqFt = u.AreaSqFt,
                Status = u.Status,
                TenantId = u.TenantId,
                OwnerId = u.OwnerId,
                IsActive = u.IsActive,
                HasBalcony = u.HasBalcony,
                HasParking = u.HasParking,
                Amenities = u.Amenities != null ? JsonSerializer.Deserialize<List<string>>(u.Amenities) : null,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return (units, totalRecords);
        }

        public async Task<(bool Success, string Message, UnitResponseDTO? Data)> CreateUnitAsync(CreateUnitDTO dto)
        {
            // Validate references
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.BuildingId == dto.BuildingId);
            if (building == null)
                return (false, $"Building with ID {dto.BuildingId} not found.", null);

            var floor = await _context.Floors.FirstOrDefaultAsync(f => f.FloorId == dto.FloorId);
            if (floor == null)
                return (false, $"Floor with ID {dto.FloorId} not found.", null);

            if (floor.BuildingId != dto.BuildingId)
                return (false, "The provided floor does not belong to the specified building.", null);

            // Optional tenant/owner existence check (if provided)
            if (dto.TenantId.HasValue)
            {
                var tenantExists = await _context.Users.AnyAsync(u => u.Id == dto.TenantId.Value);
                if (!tenantExists)
                    return (false, $"Tenant user with ID {dto.TenantId.Value} not found.", null);
            }
            if (dto.OwnerId.HasValue)
            {
                var ownerExists = await _context.Users.AnyAsync(u => u.Id == dto.OwnerId.Value);
                if (!ownerExists)
                    return (false, $"Owner user with ID {dto.OwnerId.Value} not found.", null);
            }

            // Duplicate check: UnitNumber within same floor
            var duplicate = await _context.Units.AnyAsync(u => u.FloorId == dto.FloorId && u.UnitNumber == dto.UnitNumber);
            if (duplicate)
                return (false, $"Unit number '{dto.UnitNumber}' already exists on this floor.", null);

            var unit = new Unit
            {
                FloorId = dto.FloorId,
                BuildingId = dto.BuildingId,
                UnitNumber = dto.UnitNumber,
                Type = dto.Type,
                Category = dto.Category,
                AreaSqFt = dto.AreaSqFt,
                Status = dto.Status,
                TenantId = dto.TenantId,
                OwnerId = dto.OwnerId,
                IsActive = dto.IsActive,
                HasBalcony = dto.HasBalcony,
                HasParking = dto.HasParking,
                Amenities = dto.Amenities != null ? JsonSerializer.Serialize(dto.Amenities) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            var response = new UnitResponseDTO
            {
                UnitId = unit.UnitId,
                FloorId = unit.FloorId,
                BuildingId = unit.BuildingId,
                UnitNumber = unit.UnitNumber,
                Type = unit.Type,
                Category = unit.Category,
                AreaSqFt = unit.AreaSqFt,
                Status = unit.Status,
                TenantId = unit.TenantId,
                OwnerId = unit.OwnerId,
                IsActive = unit.IsActive,
                HasBalcony = unit.HasBalcony,
                HasParking = unit.HasParking,
                Amenities = unit.Amenities != null ? JsonSerializer.Deserialize<List<string>>(unit.Amenities) : null,
                CreatedAt = unit.CreatedAt,
                UpdatedAt = unit.UpdatedAt
            };

            return (true, "Unit created successfully.", response);
        }

        public async Task<(bool Success, string Message, UnitResponseDTO? Data)> UpdateUnitAsync(int unitId, UpdateUnitDTO dto)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit == null)
                return (false, $"Unit with ID {unitId} not found.", null);

            if (dto.UnitNumber != null && dto.UnitNumber != unit.UnitNumber)
            {
                var duplicate = await _context.Units.AnyAsync(u => u.FloorId == unit.FloorId && u.UnitNumber == dto.UnitNumber && u.UnitId != unit.UnitId);
                if (duplicate)
                    return (false, $"Unit number '{dto.UnitNumber}' already exists on this floor.", null);
                unit.UnitNumber = dto.UnitNumber;
            }

            if (dto.Type != null) unit.Type = dto.Type;
            if (dto.Category != null) unit.Category = dto.Category;
            if (dto.AreaSqFt.HasValue) unit.AreaSqFt = dto.AreaSqFt.Value;
            if (dto.Status != null) unit.Status = dto.Status;
            if (dto.TenantId.HasValue) unit.TenantId = dto.TenantId.Value;
            if (dto.OwnerId.HasValue) unit.OwnerId = dto.OwnerId.Value;
            if (dto.IsActive.HasValue) unit.IsActive = dto.IsActive.Value;
            if (dto.HasBalcony.HasValue) unit.HasBalcony = dto.HasBalcony.Value;
            if (dto.HasParking.HasValue) unit.HasParking = dto.HasParking.Value;
            if (dto.Amenities != null) unit.Amenities = JsonSerializer.Serialize(dto.Amenities);

            unit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new UnitResponseDTO
            {
                UnitId = unit.UnitId,
                FloorId = unit.FloorId,
                BuildingId = unit.BuildingId,
                UnitNumber = unit.UnitNumber,
                Type = unit.Type,
                Category = unit.Category,
                AreaSqFt = unit.AreaSqFt,
                Status = unit.Status,
                TenantId = unit.TenantId,
                OwnerId = unit.OwnerId,
                IsActive = unit.IsActive,
                HasBalcony = unit.HasBalcony,
                HasParking = unit.HasParking,
                Amenities = unit.Amenities != null ? JsonSerializer.Deserialize<List<string>>(unit.Amenities) : null,
                CreatedAt = unit.CreatedAt,
                UpdatedAt = unit.UpdatedAt
            };

            return (true, "Unit updated successfully.", response);
        }

        public async Task<(bool Success, string Message)> DeleteUnitAsync(int unitId)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit == null)
                return (false, $"Unit with ID {unitId} not found.");

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return (true, "Unit deleted successfully.");
        }
    }
}
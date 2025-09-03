using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.DoorCRUD.DTOs;
using YopoBackend.Modules.DoorCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.DoorCRUD.Services
{
    /// <summary>
    /// Service for managing door operations with access control.
    /// Module ID: 12 (DoorCRUD)
    /// </summary>
    public class DoorService : BaseAccessControlService, IDoorService
    {
        /// <summary>
        /// Initializes a new instance of the DoorService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public DoorService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DoorDto>> GetAllDoorsAsync(int userId)
        {
            var query = _context.Doors
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .AsQueryable();
            
            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);
            
            var doors = await query
                .Select(d => MapToDoorDto(d))
                .ToListAsync();

            return doors;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DoorDto>> GetActiveDoorsAsync(int userId)
        {
            var query = _context.Doors
                .Where(d => d.Active)
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .AsQueryable();
            
            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);
            
            var doors = await query
                .Select(d => MapToDoorDto(d))
                .ToListAsync();

            return doors;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DoorDto>> GetDoorsByBuildingIdAsync(int buildingId, int userId)
        {
            var query = _context.Doors
                .Where(d => d.BuildingId == buildingId)
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .AsQueryable();
            
            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);
            
            var doors = await query
                .Select(d => MapToDoorDto(d))
                .ToListAsync();

            return doors;
        }

        /// <inheritdoc />
        public async Task<DoorDto?> GetDoorByIdAsync(int id, int userId)
        {
            var door = await _context.Doors
                .Where(d => d.DoorId == id)
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .FirstOrDefaultAsync();
            
            if (door == null)
            {
                return null;
            }
            
            // Check access control using base class method
            if (!await HasAccessToEntityAsync(door, userId))
            {
                return null; // User doesn't have access to this door
            }

            return MapToDoorDto(door);
        }

        /// <inheritdoc />
        public async Task<DoorDto> CreateDoorAsync(CreateDoorDto createDoorDto, int createdByUserId)
        {
            // For creation, we need to validate that the user has access to create doors
            // Since this is a new door, we check if user can access other doors they created
            // or if they have "ALL" access control
            var userDataAccessControl = await GetUserDataAccessControlAsync(createdByUserId);
            
            // If user has "OWN" access control, they can still create doors but only for buildings they have access to
            // We could add additional building-level access control here if needed
            if (userDataAccessControl == "OWN")
            {
                // For "OWN" access, check if user has created any doors for this building before
                var hasAccessToBuilding = await _context.Doors
                    .AnyAsync(d => d.BuildingId == createDoorDto.BuildingId && d.CreatedBy == createdByUserId) ||
                    await _context.Buildings
                    .AnyAsync(b => b.Id == createDoorDto.BuildingId && b.CreatedBy == createdByUserId);
                    
                if (!hasAccessToBuilding)
                {
                    // Allow creation if this is their first door in a building they created or if no access restrictions
                    var buildingCreatedByUser = await _context.Buildings
                        .AnyAsync(b => b.Id == createDoorDto.BuildingId && b.CreatedBy == createdByUserId);
                    
                    if (!buildingCreatedByUser && await _context.Buildings.AnyAsync(b => b.Id == createDoorDto.BuildingId))
                    {
                        throw new UnauthorizedAccessException("You don't have access to create doors for this building.");
                    }
                }
            }

            // Check if a similar door already exists
            var existingDoor = await DoorExistsAsync(createDoorDto.BuildingId, createDoorDto.Type, createDoorDto.Location);
            if (existingDoor)
            {
                throw new InvalidOperationException($"A door of type '{createDoorDto.Type}' already exists at location '{createDoorDto.Location}' in this building.");
            }

            var door = new Door
            {
                BuildingId = createDoorDto.BuildingId,
                Type = createDoorDto.Type,
                IntercomId = createDoorDto.IntercomId,
                CCTVId = createDoorDto.CCTVId,
                Active = createDoorDto.Active,
                FireExit = createDoorDto.FireExit,
                PinOnly = createDoorDto.PinOnly,
                CanOpenByWatchCommand = createDoorDto.CanOpenByWatchCommand,
                IsCarPark = createDoorDto.IsCarPark,
                Name = createDoorDto.Name,
                Location = createDoorDto.Location,
                Floor = createDoorDto.Floor,
                HasAutoLock = createDoorDto.HasAutoLock,
                AutoLockDelay = createDoorDto.AutoLockDelay,
                HasCardAccess = createDoorDto.HasCardAccess,
                HasBiometricAccess = createDoorDto.HasBiometricAccess,
                MaxPinAttempts = createDoorDto.MaxPinAttempts,
                LockoutDuration = createDoorDto.LockoutDuration,
                AccessLevel = createDoorDto.AccessLevel,
                OperatingHours = createDoorDto.OperatingHours,
                IsMonitored = createDoorDto.IsMonitored,
                Description = createDoorDto.Description,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow
            };

            _context.Doors.Add(door);
            await _context.SaveChangesAsync();

            // Load the created door with its related entities
            var createdDoor = await _context.Doors
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .FirstAsync(d => d.DoorId == door.DoorId);

            return MapToDoorDto(createdDoor);
        }

        /// <inheritdoc />
        public async Task<DoorDto?> UpdateDoorAsync(int id, UpdateDoorDto updateDoorDto, int userId)
        {
            var door = await _context.Doors
                .FirstOrDefaultAsync(d => d.DoorId == id);

            if (door == null)
            {
                return null; // Door not found
            }
            
            // Check access control using base class method
            if (!await HasAccessToEntityAsync(door, userId))
            {
                return null; // User doesn't have access to update this door
            }

            // If building is being changed, validate access to the new building
            if (door.BuildingId != updateDoorDto.BuildingId)
            {
                var userDataAccessControl = await GetUserDataAccessControlAsync(userId);
                if (userDataAccessControl == "OWN")
                {
                    // Check if user has access to the new building
                    var hasAccessToNewBuilding = await _context.Buildings
                        .AnyAsync(b => b.Id == updateDoorDto.BuildingId && b.CreatedBy == userId);
                    
                    if (!hasAccessToNewBuilding)
                    {
                        throw new UnauthorizedAccessException("You don't have access to move this door to the specified building.");
                    }
                }
            }

            // Check if a similar door already exists (excluding current door)
            var existingDoor = await DoorExistsAsync(updateDoorDto.BuildingId, updateDoorDto.Type, updateDoorDto.Location, id);
            if (existingDoor)
            {
                throw new InvalidOperationException($"A door of type '{updateDoorDto.Type}' already exists at location '{updateDoorDto.Location}' in this building.");
            }

            // Update door properties
            door.BuildingId = updateDoorDto.BuildingId;
            door.Type = updateDoorDto.Type;
            door.IntercomId = updateDoorDto.IntercomId;
            door.CCTVId = updateDoorDto.CCTVId;
            door.Active = updateDoorDto.Active;
            door.FireExit = updateDoorDto.FireExit;
            door.PinOnly = updateDoorDto.PinOnly;
            door.CanOpenByWatchCommand = updateDoorDto.CanOpenByWatchCommand;
            door.IsCarPark = updateDoorDto.IsCarPark;
            door.Name = updateDoorDto.Name;
            door.Location = updateDoorDto.Location;
            door.Floor = updateDoorDto.Floor;
            door.HasAutoLock = updateDoorDto.HasAutoLock;
            door.AutoLockDelay = updateDoorDto.AutoLockDelay;
            door.HasCardAccess = updateDoorDto.HasCardAccess;
            door.HasBiometricAccess = updateDoorDto.HasBiometricAccess;
            door.MaxPinAttempts = updateDoorDto.MaxPinAttempts;
            door.LockoutDuration = updateDoorDto.LockoutDuration;
            door.AccessLevel = updateDoorDto.AccessLevel;
            door.OperatingHours = updateDoorDto.OperatingHours;
            door.IsMonitored = updateDoorDto.IsMonitored;
            door.Description = updateDoorDto.Description;
            door.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load the updated door with its related entities
            var updatedDoor = await _context.Doors
                .Include(d => d.Building)
                .Include(d => d.Intercom)
                .Include(d => d.CCTV)
                .FirstAsync(d => d.DoorId == door.DoorId);

            return MapToDoorDto(updatedDoor);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDoorAsync(int id, int userId)
        {
            var door = await _context.Doors
                .FirstOrDefaultAsync(d => d.DoorId == id);

            if (door == null)
            {
                return false; // Door not found
            }
            
            // Check access control using base class method
            if (!await HasAccessToEntityAsync(door, userId))
            {
                return false; // User doesn't have access to delete this door
            }

            _context.Doors.Remove(door);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DoorExistsAsync(int buildingId, string type, string? location = null, int? excludeId = null)
        {
            var query = _context.Doors
                .Where(d => d.BuildingId == buildingId && d.Type == type);

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(d => d.Location == location);
            }

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.DoorId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Maps a Door entity to a DoorDto.
        /// </summary>
        /// <param name="door">The door entity to map.</param>
        /// <returns>The mapped DoorDto.</returns>
        private static DoorDto MapToDoorDto(Door door)
        {
            return new DoorDto
            {
                DoorId = door.DoorId,
                BuildingId = door.BuildingId,
                BuildingName = door.Building?.Name,
                Type = door.Type,
                IntercomId = door.IntercomId,
                IntercomName = door.Intercom?.Name,
                CCTVId = door.CCTVId,
                CCTVName = door.CCTV?.Name,
                DateCreated = door.DateCreated,
                Active = door.Active,
                FireExit = door.FireExit,
                PinOnly = door.PinOnly,
                CanOpenByWatchCommand = door.CanOpenByWatchCommand,
                IsCarPark = door.IsCarPark,
                Name = door.Name,
                Location = door.Location,
                Floor = door.Floor,
                HasAutoLock = door.HasAutoLock,
                AutoLockDelay = door.AutoLockDelay,
                HasCardAccess = door.HasCardAccess,
                HasBiometricAccess = door.HasBiometricAccess,
                MaxPinAttempts = door.MaxPinAttempts,
                LockoutDuration = door.LockoutDuration,
                AccessLevel = door.AccessLevel,
                OperatingHours = door.OperatingHours,
                IsMonitored = door.IsMonitored,
                Description = door.Description,
                CreatedBy = door.CreatedBy,
                CreatedAt = door.CreatedAt,
                UpdatedAt = door.UpdatedAt
            };
        }
    }
}

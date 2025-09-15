using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Services;
using YopoBackend.Constants;

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

            // Check access control considering Super Admin status
            if (!await HasAccessToBuildingAsync(id, userId))
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

            // Automatically create a user-building permission for the creator
            var userBuildingPermission = new UserBuildingPermission
            {
                UserId = createdByUserId,
                BuildingId = building.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.UserBuildingPermissions.Add(userBuildingPermission);
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

            // Check access control considering Super Admin status
            if (!await HasAccessToBuildingAsync(id, userId))
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

            // Check access control considering Super Admin status
            if (!await HasAccessToBuildingAsync(id, userId))
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
        /// Super Admin users get access to all buildings, bypassing all access control.
        /// Other users are subject to both DataAccessControl (OWN/ALL) and user-building permissions.
        /// </summary>
        /// <param name="userId">The ID of the user requesting access.</param>
        /// <param name="includeInactive">Whether to include inactive buildings.</param>
        /// <returns>List of buildings the user has access to.</returns>
        private async Task<List<Building>> GetBuildingsBasedOnUserAccess(int userId, bool includeInactive)
        {
            var query = _context.Buildings.AsQueryable();

            // Check if user is Super Admin - if so, bypass ALL access control
            if (await IsUserSuperAdminAsync(userId))
            {
                Console.WriteLine($"Super Admin access: User {userId} bypassing all access control for buildings");
                // Super Admin gets access to ALL buildings with NO filtering except active/inactive
                if (!includeInactive)
                {
                    query = query.Where(b => b.IsActive);
                }
            }
            else
            {
                Console.WriteLine($"Regular user access: User {userId} applying access control for buildings");
                // For non-super admin users, apply both DataAccessControl and user-building permissions
                
                // First, apply standard access control based on DataAccessControl setting ("OWN" vs "ALL")
                query = await ApplyAccessControlAsync(query, userId);

                // Then, apply user-building permission filtering
                query = await ApplyUserBuildingPermissionFilterAsync(query, userId);

                // Finally, apply active/inactive filter
                if (!includeInactive)
                {
                    query = query.Where(b => b.IsActive);
                }
            }

            return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Checks if a user is a Super Admin.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <returns>True if the user is a Super Admin, false otherwise.</returns>
        private async Task<bool> IsUserSuperAdminAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.UserType?.Id == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
        }

        /// <summary>
        /// Applies user-building permission filtering to the query.
        /// Only returns buildings the user has explicit permission to access.
        /// </summary>
        /// <param name="query">The buildings query to filter.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Filtered query with user-building permission restrictions.</returns>
        private async Task<IQueryable<Building>> ApplyUserBuildingPermissionFilterAsync(IQueryable<Building> query, int userId)
        {
            // Get building IDs that the user has permission to access
            var permittedBuildingIds = await _context.UserBuildingPermissions
                .Where(ubp => ubp.UserId == userId && ubp.IsActive)
                .Select(ubp => ubp.BuildingId)
                .ToListAsync();

            // Filter buildings by user permissions
            return query.Where(b => permittedBuildingIds.Contains(b.Id));
        }

        /// <summary>
        /// Checks if a user has access to a specific building, considering Super Admin status.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>True if the user has access to the building, false otherwise.</returns>
        private async Task<bool> HasAccessToBuildingAsync(int buildingId, int userId)
        {
            // Super Admin has access to all buildings
            if (await IsUserSuperAdminAsync(userId))
            {
                return true;
            }

            // Check if user has explicit permission to this building
            return await _context.UserBuildingPermissions
                .AnyAsync(ubp => ubp.UserId == userId && ubp.BuildingId == buildingId && ubp.IsActive);
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

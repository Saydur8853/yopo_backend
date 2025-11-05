using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.FloorCRUD.DTOs;
using YopoBackend.Modules.UnitCRUD.DTOs;
using YopoBackend.Modules.AmenityCRUD.DTOs;
using YopoBackend.Services;

namespace YopoBackend.Modules.BuildingCRUD.Services
{
    /// <summary>
    /// Service for managing building operations with PM data access control.
    /// Module ID: 4 (BuildingCRUD)
    /// Data Access Control: PM (Super Admin sees all, Property Manager sees own customer buildings)
    /// </summary>
    public class BuildingService : BaseAccessControlService, IBuildingService
    {
        /// <summary>
        /// Initializes a new instance of the BuildingService class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public BuildingService(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all buildings with pagination and filtering based on user's data access control.
        /// </summary>
        public async Task<BuildingListResponseDTO> GetBuildingsAsync(
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            int? customerId = null,
            int? buildingId = null,
            bool? isActive = null)
        {
            // Start with base query
            var query = _context.Buildings
                .Include(b => b.Customer)
                .Include(b => b.CreatedByUser)
                .AsQueryable();

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            // Get current user to check their type
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            
            // SuperAdmin sees all buildings - skip building permission checks
            if (currentUser?.UserTypeId != YopoBackend.Constants.UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                // If the user has explicit building permissions, restrict to those
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == currentUserId && p.IsActive)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                if (explicitBuildingIds.Any())
                {
                    query = query.Where(b => explicitBuildingIds.Contains(b.BuildingId));
                }
                else
                {
                    // If invited non-PM with no explicit permissions, return none
                    if (currentUser != null && currentUser.UserTypeId != YopoBackend.Constants.UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && currentUser.InviteById.HasValue)
                    {
                        query = query.Where(b => false);
                    }
                }
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm) || b.Address.Contains(searchTerm));
            }

            if (customerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == customerId.Value);
            }

            if (buildingId.HasValue)
            {
                query = query.Where(b => b.BuildingId == buildingId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }


            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var buildings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BuildingResponseDTO
                {
                    BuildingId = b.BuildingId,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer.CustomerName,
                    CompanyName = b.Customer.CompanyName ?? string.Empty,
                    Name = b.Name,
                    Address = b.Address,
                    IsActive = b.IsActive,
                    CreatedByName = b.CreatedByUser.Name,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Floors = new List<FloorResponseDTO>(),
                    Units = new List<UnitResponseDTO>(),
                    Amenities = new List<AmenityResponseDTO>()
                })
                .ToListAsync();

            // Populate related data: Floors, Units, Amenities for the fetched buildings
            var buildingIds = buildings.Select(b => b.BuildingId).ToList();

            // Floors
            var floors = await _context.Floors
                .Where(f => buildingIds.Contains(f.BuildingId))
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

            var floorsByBuilding = floors
                .GroupBy(f => f.BuildingId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Units (fetch amenities JSON as string then map in-memory)
            var unitRows = await _context.Units
                .Where(u => buildingIds.Contains(u.BuildingId))
                .Select(u => new
                {
                    u.UnitId,
                    u.FloorId,
                    u.BuildingId,
                    u.UnitNumber,
                    u.Type,
                    u.Category,
                    u.AreaSqFt,
                    u.Status,
                    u.TenantId,
                    u.OwnerId,
                    u.IsActive,
                    u.HasBalcony,
                    u.HasParking,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            var units = unitRows.Select(u => new UnitResponseDTO
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
                Amenities = new List<string>(),
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            var unitsByBuilding = units
                .GroupBy(u => u.BuildingId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Amenities
            var amenities = await _context.Amenities
                .Where(a => buildingIds.Contains(a.BuildingId))
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

            var amenitiesByBuilding = amenities
                .GroupBy(a => a.BuildingId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Assign to each building DTO
            foreach (var b in buildings)
            {
                if (floorsByBuilding.TryGetValue(b.BuildingId, out var bf)) b.Floors = bf; else b.Floors = new List<FloorResponseDTO>();
                if (unitsByBuilding.TryGetValue(b.BuildingId, out var bu)) b.Units = bu; else b.Units = new List<UnitResponseDTO>();
                if (amenitiesByBuilding.TryGetValue(b.BuildingId, out var ba)) b.Amenities = ba; else b.Amenities = new List<AmenityResponseDTO>();
            }

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new BuildingListResponseDTO
            {
                Buildings = buildings,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }


        /// <summary>
        /// Creates a new building.
        /// </summary>
        public async Task<BuildingResponseDTO> CreateBuildingAsync(CreateBuildingDTO createBuildingDto, int currentUserId)
        {
            // Enforce: Only Property Managers can create buildings
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null || currentUser.UserTypeId != YopoBackend.Constants.UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                throw new UnauthorizedAccessException("Only Property Managers can create buildings.");
            }

            // Determine target customerId based on current user's access level (PM)
            var userDataAccessControl = await GetUserDataAccessControlAsync(currentUserId);
            int targetCustomerId;

            // For PM (and any legacy ALL/OWN), derive PM's customerId from token/user
            var propertyManagerId = await FindPropertyManagerForUserAsync(currentUserId) ?? currentUserId;
            targetCustomerId = propertyManagerId;

            // Validate that the customer exists and is accessible
            var hasCustomerAccess = await ValidateCustomerAccessAsync(targetCustomerId, currentUserId);
            if (!hasCustomerAccess)
            {
                throw new UnauthorizedAccessException("Access denied to the specified customer.");
            }

            var building = new Building
            {
                CustomerId = targetCustomerId,
                Name = createBuildingDto.Name,
                Address = createBuildingDto.Address,
                IsActive = true, // New buildings are active by default
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // Load the created building with navigation properties
            var createdBuilding = await _context.Buildings
                .Include(b => b.Customer)
                .Include(b => b.CreatedByUser)
                .FirstOrDefaultAsync(b => b.BuildingId == building.BuildingId);

            return new BuildingResponseDTO
            {
                BuildingId = createdBuilding!.BuildingId,
                CustomerId = createdBuilding.CustomerId,
                CustomerName = createdBuilding.Customer.CustomerName,
                CompanyName = createdBuilding.Customer.CompanyName ?? string.Empty,
                Name = createdBuilding.Name,
                Address = createdBuilding.Address,
                IsActive = createdBuilding.IsActive,
                CreatedByName = createdBuilding.CreatedByUser.Name,
                CreatedAt = createdBuilding.CreatedAt,
                UpdatedAt = createdBuilding.UpdatedAt,
                Floors = new List<FloorResponseDTO>(),
                Units = new List<UnitResponseDTO>(),
                Amenities = new List<AmenityResponseDTO>()
            };
        }

        /// <summary>
        /// Updates an existing building with access control validation.
        /// </summary>
        public async Task<BuildingResponseDTO?> UpdateBuildingAsync(int buildingId, UpdateBuildingDTO updateBuildingDto, int currentUserId)
        {
            var query = _context.Buildings
                .Include(b => b.Customer)
                .Include(b => b.CreatedByUser)
                .Where(b => b.BuildingId == buildingId);

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            // Get current user to check their type
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            
            // SuperAdmin sees all buildings - skip building permission checks
            if (currentUser?.UserTypeId != YopoBackend.Constants.UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                // If the user has explicit building permissions, restrict to those
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == currentUserId && p.IsActive)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                if (explicitBuildingIds.Any())
                {
                    query = query.Where(b => explicitBuildingIds.Contains(b.BuildingId));
                }
                else
                {
                    if (currentUser != null && currentUser.UserTypeId != YopoBackend.Constants.UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && currentUser.InviteById.HasValue)
                    {
                        query = query.Where(b => false);
                    }
                }
            }

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                return null;

            // Update building properties only if provided (partial update support)
            if (updateBuildingDto.Name != null)
                building.Name = updateBuildingDto.Name;
            
            if (updateBuildingDto.Address != null)
                building.Address = updateBuildingDto.Address;
            
            
            if (updateBuildingDto.IsActive.HasValue)
                building.IsActive = updateBuildingDto.IsActive.Value;
            
            building.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BuildingResponseDTO
            {
                BuildingId = building.BuildingId,
                CustomerId = building.CustomerId,
                CustomerName = building.Customer.CustomerName,
                CompanyName = building.Customer.CompanyName ?? string.Empty,
                Name = building.Name,
                Address = building.Address,
                IsActive = building.IsActive,
                CreatedByName = building.CreatedByUser.Name,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt,
                Floors = new List<FloorResponseDTO>(),
                Units = new List<UnitResponseDTO>(),
                Amenities = new List<AmenityResponseDTO>()
            };
        }

        /// <summary>
        /// Deletes a building with access control validation.
        /// </summary>
        public async Task<bool> DeleteBuildingAsync(int buildingId, int currentUserId)
        {
            var query = _context.Buildings.Where(b => b.BuildingId == buildingId);

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            // Get current user to check their type
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            
            // SuperAdmin sees all buildings - skip building permission checks
            if (currentUser?.UserTypeId != YopoBackend.Constants.UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                // If the user has explicit building permissions, restrict to those
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == currentUserId && p.IsActive)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                if (explicitBuildingIds.Any())
                {
                    query = query.Where(b => explicitBuildingIds.Contains(b.BuildingId));
                }
                else
                {
                    if (currentUser != null && currentUser.UserTypeId != YopoBackend.Constants.UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && currentUser.InviteById.HasValue)
                    {
                        query = query.Where(b => false);
                    }
                }
            }

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                return false;

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return true;
        }



        /// <summary>
        /// Validates if a customer ID is accessible by the current user based on PM access control.
        /// </summary>
        public async Task<bool> ValidateCustomerAccessAsync(int customerId, int currentUserId)
        {
            // Check if customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            if (!customerExists)
                return false;

            // Get user's access control setting
            var userDataAccessControl = await GetUserDataAccessControlAsync(currentUserId);

            if (userDataAccessControl == "OWN")
            {
                // For "OWN" access, user can only access customers they created
                return await _context.Customers.AnyAsync(c => c.CustomerId == customerId && c.CreatedBy == currentUserId);
            }

            // For "ALL" access or null, user can access all customers
            return true;
        }
    }
}
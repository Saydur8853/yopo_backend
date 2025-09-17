using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.DTOs;
using YopoBackend.Modules.BuildingCRUD.Models;
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
            bool? isActive = null,
            bool? hasGym = null,
            bool? hasSwimmingPool = null,
            bool? hasSauna = null)
        {
            // Start with base query
            var query = _context.Buildings
                .Include(b => b.Customer)
                .Include(b => b.CreatedByUser)
                .AsQueryable();

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm) || b.Address.Contains(searchTerm));
            }

            if (customerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == customerId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            if (hasGym.HasValue)
            {
                query = query.Where(b => b.HasGym == hasGym.Value);
            }

            if (hasSwimmingPool.HasValue)
            {
                query = query.Where(b => b.HasSwimmingPool == hasSwimmingPool.Value);
            }

            if (hasSauna.HasValue)
            {
                query = query.Where(b => b.HasSauna == hasSauna.Value);
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
                    Floors = b.Floors,
                    ParkingFloor = b.ParkingFloor,
                    HasGym = b.HasGym,
                    HasSwimmingPool = b.HasSwimmingPool,
                    HasSauna = b.HasSauna,
                    IsActive = b.IsActive,
                    CreatedByName = b.CreatedByUser.Name,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

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
        /// Gets a building by ID with access control validation.
        /// </summary>
        public async Task<BuildingResponseDTO?> GetBuildingByIdAsync(int buildingId, int currentUserId)
        {
            var query = _context.Buildings
                .Include(b => b.Customer)
                .Include(b => b.CreatedByUser)
                .Where(b => b.BuildingId == buildingId);

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            var building = await query.FirstOrDefaultAsync();

            if (building == null)
                return null;

            return new BuildingResponseDTO
            {
                BuildingId = building.BuildingId,
                CustomerId = building.CustomerId,
                CustomerName = building.Customer.CustomerName,
                CompanyName = building.Customer.CompanyName ?? string.Empty,
                Name = building.Name,
                Address = building.Address,
                Floors = building.Floors,
                ParkingFloor = building.ParkingFloor,
                HasGym = building.HasGym,
                HasSwimmingPool = building.HasSwimmingPool,
                HasSauna = building.HasSauna,
                IsActive = building.IsActive,
                CreatedByName = building.CreatedByUser.Name,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };
        }

        /// <summary>
        /// Creates a new building.
        /// </summary>
        public async Task<BuildingResponseDTO> CreateBuildingAsync(CreateBuildingDTO createBuildingDto, int currentUserId)
        {
            // Validate that the customer exists and is accessible
            var hasCustomerAccess = await ValidateCustomerAccessAsync(createBuildingDto.CustomerId, currentUserId);
            if (!hasCustomerAccess)
            {
                throw new UnauthorizedAccessException("Access denied to the specified customer.");
            }

            var building = new Building
            {
                CustomerId = createBuildingDto.CustomerId,
                Name = createBuildingDto.Name,
                Address = createBuildingDto.Address,
                Floors = createBuildingDto.Floors,
                ParkingFloor = createBuildingDto.ParkingFloor,
                HasGym = createBuildingDto.HasGym,
                HasSwimmingPool = createBuildingDto.HasSwimmingPool,
                HasSauna = createBuildingDto.HasSauna,
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
                Floors = createdBuilding.Floors,
                ParkingFloor = createdBuilding.ParkingFloor,
                HasGym = createdBuilding.HasGym,
                HasSwimmingPool = createdBuilding.HasSwimmingPool,
                HasSauna = createdBuilding.HasSauna,
                IsActive = createdBuilding.IsActive,
                CreatedByName = createdBuilding.CreatedByUser.Name,
                CreatedAt = createdBuilding.CreatedAt,
                UpdatedAt = createdBuilding.UpdatedAt
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

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                return null;

            // Update building properties
            building.Name = updateBuildingDto.Name;
            building.Address = updateBuildingDto.Address;
            building.Floors = updateBuildingDto.Floors;
            building.ParkingFloor = updateBuildingDto.ParkingFloor;
            building.HasGym = updateBuildingDto.HasGym;
            building.HasSwimmingPool = updateBuildingDto.HasSwimmingPool;
            building.HasSauna = updateBuildingDto.HasSauna;
            building.IsActive = updateBuildingDto.IsActive;
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
                Floors = building.Floors,
                ParkingFloor = building.ParkingFloor,
                HasGym = building.HasGym,
                HasSwimmingPool = building.HasSwimmingPool,
                HasSauna = building.HasSauna,
                IsActive = building.IsActive,
                CreatedByName = building.CreatedByUser.Name,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
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

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                return false;

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Gets building amenities summary for a specific building.
        /// </summary>
        public async Task<BuildingAmenitiesDTO?> GetBuildingAmenitiesAsync(int buildingId, int currentUserId)
        {
            var query = _context.Buildings.Where(b => b.BuildingId == buildingId);

            // Apply access control
            query = await ApplyAccessControlAsync(query, currentUserId);

            var building = await query.FirstOrDefaultAsync();
            if (building == null)
                return null;

            var amenities = new List<string>();
            if (building.HasGym) amenities.Add("Gym");
            if (building.HasSwimmingPool) amenities.Add("Swimming Pool");
            if (building.HasSauna) amenities.Add("Sauna");

            return new BuildingAmenitiesDTO
            {
                BuildingId = building.BuildingId,
                Name = building.Name,
                Amenities = amenities,
                AmenityCount = amenities.Count
            };
        }

        /// <summary>
        /// Gets buildings for a specific customer with access control validation.
        /// </summary>
        public async Task<BuildingListResponseDTO> GetBuildingsByCustomerAsync(
            int customerId,
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            bool? isActive = null)
        {
            // Validate customer access first
            var hasCustomerAccess = await ValidateCustomerAccessAsync(customerId, currentUserId);
            if (!hasCustomerAccess)
            {
                // Return empty result if no access to customer
                return new BuildingListResponseDTO
                {
                    Buildings = new List<BuildingResponseDTO>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false
                };
            }

            return await GetBuildingsAsync(currentUserId, page, pageSize, null, customerId, isActive);
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
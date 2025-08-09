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
            // Check if buildings already exist
            var existingBuildingsCount = await _context.Buildings.CountAsync();
            if (existingBuildingsCount > 0)
            {
                // Buildings already exist, skip initialization
                return;
            }

            var sampleBuildings = new List<Building>
            {
                new Building
                {
                    Name = "Downtown Plaza",
                    Address = "123 Main Street, Downtown, NY 10001",
                    Photo = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Building
                {
                    Name = "Sunset Apartments",
                    Address = "456 Elm Avenue, Westside, CA 90210",
                    Photo = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Building
                {
                    Name = "Tech Hub Center",
                    Address = "789 Innovation Drive, Silicon Valley, CA 94105",
                    Photo = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Building
                {
                    Name = "Green Valley Towers",
                    Address = "321 Oak Street, Green Valley, TX 75001",
                    Photo = "https://images.unsplash.com/photo-1582268611958-ebfd161ef9cf?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                },
                new Building
                {
                    Name = "Harbor View Complex",
                    Address = "654 Waterfront Boulevard, Harbor City, FL 33101",
                    Photo = "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Building
                {
                    Name = "Mountain View Residences",
                    Address = "987 Highland Road, Mountain View, CO 80424",
                    Photo = "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Building
                {
                    Name = "City Square Mall",
                    Address = "147 Commerce Street, City Center, IL 60601",
                    Photo = "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Building
                {
                    Name = "Riverside Office Park",
                    Address = "258 River Road, Riverside, WA 98052",
                    Photo = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Building
                {
                    Name = "Skyline Condominiums",
                    Address = "369 Sky Drive, Uptown, NY 10128",
                    Photo = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Building
                {
                    Name = "Industrial Park East",
                    Address = "741 Factory Lane, Industrial District, MI 48201",
                    Photo = "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Building
                {
                    Name = "Lakeside Villas",
                    Address = "852 Lakeshore Drive, Lakewood, MN 55416",
                    Photo = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Building
                {
                    Name = "Metro Business Center",
                    Address = "963 Corporate Boulevard, Metro Area, GA 30309",
                    Photo = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Building
                {
                    Name = "Parkside Gardens",
                    Address = "174 Garden Avenue, Parkside, OR 97201",
                    Photo = "https://images.unsplash.com/photo-1600607687644-c7171b42498b?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Building
                {
                    Name = "Heritage Square",
                    Address = "285 Heritage Street, Old Town, MA 02101",
                    Photo = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop",
                    IsActive = false, // One inactive building
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Building
                {
                    Name = "Future City Complex",
                    Address = "396 Tomorrow Lane, New Development, AZ 85001",
                    Photo = "https://images.unsplash.com/photo-1582268611958-ebfd161ef9cf?w=800&h=600&fit=crop",
                    IsActive = true,
                    CreatedBy = 1, // Assign to first user (Super Admin)
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Buildings.AddRange(sampleBuildings);
            await _context.SaveChangesAsync();
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
                IsActive = building.IsActive,
                CreatedBy = building.CreatedBy,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };
        }
    }
}

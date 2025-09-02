using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.CCTVcrud.DTOs;
using YopoBackend.Modules.CCTVcrud.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.CCTVcrud.Services
{
    /// <summary>
    /// Service implementation for CCTV operations.
    /// Module ID: 8 (CCTVcrud)
    /// </summary>
    public class CCTVService : BaseAccessControlService, ICCTVService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CCTVService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public CCTVService(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVDto>> GetAllCCTVsAsync(int userId)
        {
            var query = await GetCCTVsBasedOnUserAccess(userId, includeInactive: true);
            var cctvs = await query.ToListAsync();
            return cctvs.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVDto>> GetActiveCCTVsAsync(int userId)
        {
            var query = await GetCCTVsBasedOnUserAccess(userId, includeInactive: false);
            var cctvs = await query.ToListAsync();
            return cctvs.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVSummaryDto>> GetPublicCCTVsAsync()
        {
            var publicCCTVs = await _context.CCTVs
                .Where(c => c.IsPublic && c.IsActive)
                .Include(c => c.Building)
                .Include(c => c.Tenant)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return publicCCTVs.Select(MapToSummaryDto);
        }

        /// <inheritdoc/>
        public async Task<CCTVDto?> GetCCTVByIdAsync(int id, int userId)
        {
            var cctv = await _context.CCTVs
                .Include(c => c.Building)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.CctvId == id);

            if (cctv == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(cctv, userId))
            {
                return null; // User doesn't have access to this CCTV
            }

            return MapToDto(cctv);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVDto>> GetCCTVsByBuildingIdAsync(int buildingId, int userId)
        {
            // First check if user has access to the building
            if (!await ValidateBuildingAccessAsync(buildingId, userId))
            {
                return Enumerable.Empty<CCTVDto>();
            }

            var query = await GetCCTVsBasedOnUserAccess(userId, includeInactive: true);
            var cctvs = await query
                .Where(c => c.BuildingId == buildingId)
                .ToListAsync();

            return cctvs.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVDto>> GetCCTVsByTenantIdAsync(int tenantId, int userId)
        {
            // First check if user has access to the tenant
            if (!await ValidateTenantAccessAsync(tenantId, userId))
            {
                return Enumerable.Empty<CCTVDto>();
            }

            var query = await GetCCTVsBasedOnUserAccess(userId, includeInactive: true);
            var cctvs = await query
                .Where(c => c.TenantId == tenantId)
                .ToListAsync();

            return cctvs.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<CCTVDto> CreateCCTVAsync(CreateCCTVDto createCCTVDto, int createdByUserId)
        {
            // Validate building access
            if (!await ValidateBuildingAccessAsync(createCCTVDto.BuildingId, createdByUserId))
            {
                throw new UnauthorizedAccessException("You don't have access to the specified building.");
            }

            // Validate tenant access if tenant is specified
            if (createCCTVDto.TenantId.HasValue && 
                !await ValidateTenantAccessAsync(createCCTVDto.TenantId.Value, createdByUserId))
            {
                throw new UnauthorizedAccessException("You don't have access to the specified tenant.");
            }

            var cctv = new CCTV
            {
                Name = createCCTVDto.Name,
                Model = createCCTVDto.Model,
                Size = createCCTVDto.Size,
                BuildingId = createCCTVDto.BuildingId,
                Location = createCCTVDto.Location,
                Stream = createCCTVDto.Stream,
                IsPublic = createCCTVDto.IsPublic,
                TenantId = createCCTVDto.TenantId,
                Resolution = createCCTVDto.Resolution,
                HasNightVision = createCCTVDto.HasNightVision,
                HasPTZ = createCCTVDto.HasPTZ,
                IsActive = createCCTVDto.IsActive,
                InstallationDate = createCCTVDto.InstallationDate,
                LastMaintenanceDate = createCCTVDto.LastMaintenanceDate,
                Description = createCCTVDto.Description,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CCTVs.Add(cctv);
            await _context.SaveChangesAsync();

            // Load the created CCTV with related data
            var createdCCTV = await _context.CCTVs
                .Include(c => c.Building)
                .Include(c => c.Tenant)
                .FirstAsync(c => c.CctvId == cctv.CctvId);

            return MapToDto(createdCCTV);
        }

        /// <inheritdoc/>
        public async Task<CCTVDto?> UpdateCCTVAsync(int id, UpdateCCTVDto updateCCTVDto, int userId)
        {
            var cctv = await _context.CCTVs
                .Include(c => c.Building)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync(c => c.CctvId == id);

            if (cctv == null)
            {
                return null;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(cctv, userId))
            {
                return null; // User doesn't have access to update this CCTV
            }

            // Validate building access if building is being changed
            if (cctv.BuildingId != updateCCTVDto.BuildingId && 
                !await ValidateBuildingAccessAsync(updateCCTVDto.BuildingId, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to the specified building.");
            }

            // Validate tenant access if tenant is being changed
            if (updateCCTVDto.TenantId.HasValue && 
                cctv.TenantId != updateCCTVDto.TenantId &&
                !await ValidateTenantAccessAsync(updateCCTVDto.TenantId.Value, userId))
            {
                throw new UnauthorizedAccessException("You don't have access to the specified tenant.");
            }

            cctv.Name = updateCCTVDto.Name;
            cctv.Model = updateCCTVDto.Model;
            cctv.Size = updateCCTVDto.Size;
            cctv.BuildingId = updateCCTVDto.BuildingId;
            cctv.Location = updateCCTVDto.Location;
            cctv.Stream = updateCCTVDto.Stream;
            cctv.IsPublic = updateCCTVDto.IsPublic;
            cctv.TenantId = updateCCTVDto.TenantId;
            cctv.Resolution = updateCCTVDto.Resolution;
            cctv.HasNightVision = updateCCTVDto.HasNightVision;
            cctv.HasPTZ = updateCCTVDto.HasPTZ;
            cctv.IsActive = updateCCTVDto.IsActive;
            cctv.InstallationDate = updateCCTVDto.InstallationDate;
            cctv.LastMaintenanceDate = updateCCTVDto.LastMaintenanceDate;
            cctv.Description = updateCCTVDto.Description;
            cctv.UpdatedAt = DateTime.UtcNow;

            _context.CCTVs.Update(cctv);
            await _context.SaveChangesAsync();

            return MapToDto(cctv);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCCTVAsync(int id, int userId)
        {
            var cctv = await _context.CCTVs.FirstOrDefaultAsync(c => c.CctvId == id);
            if (cctv == null)
            {
                return false;
            }

            // Check access control using base class method
            if (!await HasAccessToEntityAsync(cctv, userId))
            {
                return false; // User doesn't have access to delete this CCTV
            }

            _context.CCTVs.Remove(cctv);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> CCTVExistsInBuildingAsync(string name, int buildingId, int? excludeId = null)
        {
            var query = _context.CCTVs.Where(c => 
                c.Name.ToLower() == name.ToLower() && 
                c.BuildingId == buildingId);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CctvId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateBuildingAccessAsync(int buildingId, int userId)
        {
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId);
            if (building == null)
            {
                return false;
            }

            return await HasAccessToEntityAsync(building, userId);
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTenantAccessAsync(int tenantId, int userId)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (tenant == null)
            {
                return false;
            }

            // Check if user has access to tenant's building first
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == tenant.BuildingId);
            if (building == null)
            {
                return false;
            }

            return await HasAccessToEntityAsync(building, userId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CCTVDto>> GetCCTVsForMonitoringAsync(int userId, int? buildingId = null)
        {
            var query = await GetCCTVsBasedOnUserAccess(userId, includeInactive: false);

            // Filter by building if specified
            if (buildingId.HasValue)
            {
                query = query.Where(c => c.BuildingId == buildingId.Value);
            }

            // Only return CCTVs with stream URLs for monitoring
            var cctvs = await query
                .Where(c => !string.IsNullOrEmpty(c.Stream))
                .OrderBy(c => c.BuildingId)
                .ThenBy(c => c.Location)
                .ToListAsync();

            return cctvs.Select(MapToDto);
        }

        /// <summary>
        /// Gets CCTVs based on user's access control settings.
        /// </summary>
        /// <param name="userId">The ID of the user requesting access.</param>
        /// <param name="includeInactive">Whether to include inactive CCTVs.</param>
        /// <returns>Queryable of CCTVs the user has access to.</returns>
        private async Task<IQueryable<CCTV>> GetCCTVsBasedOnUserAccess(int userId, bool includeInactive)
        {
            var query = _context.CCTVs
                .Include(c => c.Building)
                .Include(c => c.Tenant)
                .AsQueryable();

            // Apply access control using base class method
            query = await ApplyAccessControlAsync(query, userId);

            // Apply active/inactive filter
            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return query.OrderByDescending(c => c.CreatedAt);
        }

        /// <summary>
        /// Maps a CCTV entity to a CCTVDto.
        /// </summary>
        /// <param name="cctv">The CCTV entity to map.</param>
        /// <returns>The mapped CCTV DTO.</returns>
        private static CCTVDto MapToDto(CCTV cctv)
        {
            return new CCTVDto
            {
                CctvId = cctv.CctvId,
                Name = cctv.Name,
                Model = cctv.Model,
                Size = cctv.Size,
                BuildingId = cctv.BuildingId,
                Location = cctv.Location,
                Stream = cctv.Stream,
                IsPublic = cctv.IsPublic,
                TenantId = cctv.TenantId,
                Resolution = cctv.Resolution,
                HasNightVision = cctv.HasNightVision,
                HasPTZ = cctv.HasPTZ,
                IsActive = cctv.IsActive,
                InstallationDate = cctv.InstallationDate,
                LastMaintenanceDate = cctv.LastMaintenanceDate,
                Description = cctv.Description,
                CreatedBy = cctv.CreatedBy,
                CreatedAt = cctv.CreatedAt,
                UpdatedAt = cctv.UpdatedAt,
                BuildingName = cctv.Building?.Name,
                TenantName = cctv.Tenant?.Name
            };
        }

        /// <summary>
        /// Maps a CCTV entity to a CCTVSummaryDto.
        /// </summary>
        /// <param name="cctv">The CCTV entity to map.</param>
        /// <returns>The mapped CCTV summary DTO.</returns>
        private static CCTVSummaryDto MapToSummaryDto(CCTV cctv)
        {
            return new CCTVSummaryDto
            {
                CctvId = cctv.CctvId,
                Name = cctv.Name,
                Model = cctv.Model,
                BuildingId = cctv.BuildingId,
                Location = cctv.Location,
                IsPublic = cctv.IsPublic,
                IsActive = cctv.IsActive,
                BuildingName = cctv.Building?.Name,
                TenantName = cctv.Tenant?.Name
            };
        }
    }
}

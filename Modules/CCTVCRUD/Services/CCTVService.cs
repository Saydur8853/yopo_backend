using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.CCTVCRUD.DTOs;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.IntercomCRUD.Services;
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.CCTVCRUD.Services
{
    public interface ICCTVService
    {
        Task<(IEnumerable<CCTVResponseDto> Items, int TotalCount)> GetAllCCTVsAsync(int currentUserId, int page, int pageSize, string? searchTerm = null, int? buildingId = null, bool? isActive = null);
        Task<CCTVResponseDto?> GetCCTVByIdAsync(int id, int currentUserId);
        Task<CCTVResponseDto> CreateCCTVAsync(CreateCCTVDto createDto, int createdByUserId);
        Task<CCTVResponseDto?> UpdateCCTVAsync(int id, UpdateCCTVDto updateDto, int currentUserId);
        Task<bool> DeleteCCTVAsync(int id, int currentUserId);
    }

    public class CCTVService : BaseAccessControlService, ICCTVService
    {
        public CCTVService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<CCTVResponseDto> Items, int TotalCount)> GetAllCCTVsAsync(int currentUserId, int page, int pageSize, string? searchTerm = null, int? buildingId = null, bool? isActive = null)
        {
            var user = await GetUserWithAccessControlAsync(currentUserId);
            if (user == null) return (new List<CCTVResponseDto>(), 0);

            var query = _context.Intercoms
                .Include(i => i.Building)
                .Where(i => i.HasCCTV)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(i => i.IntercomName.ToLower().Contains(term) 
                                      || (i.InstalledLocation != null && i.InstalledLocation.ToLower().Contains(term)));
            }

            if (buildingId.HasValue)
            {
                query = query.Where(i => i.BuildingId == buildingId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(i => i.IsActive == isActive.Value);
            }
            else
            {
                // Default behavior if not specified: show active only? 
                // The original code had .Where(i => i.HasCCTV && i.IsActive)
                // But usually filters override defaults. Let's keep the original default if not specified?
                // Actually, standard pattern is usually: if filter is null, return all (or default).
                // Original code: .Where(i => i.HasCCTV && i.IsActive)
                // Let's assume if isActive is null, we default to true (Active only) to match previous behavior, OR we return all?
                // The user asked for "filter option", implying they might want to see inactive ones too.
                // However, the original code strictly filtered `i.IsActive`.
                // Let's make it: if isActive is null, we stick to `IsActive == true` to preserve backward compatibility unless user explicitly asks for all?
                // Wait, usually `isActive = null` means "don't filter by active status" (show both).
                // BUT, the previous hardcoded behavior was `IsActive`.
                // Let's stick to: if `isActive` is provided, use it. If NOT provided, default to `IsActive = true` to match previous behavior?
                // Or maybe just remove the hardcoded `IsActive` and let the filter handle it.
                // If I remove `&& i.IsActive` from the base query, then `isActive = null` would return both active and inactive.
                // This is likely what "filter option" implies.
                // BUT, for safety, let's default to `true` if not specified, effectively keeping the old behavior as default.
                
                // Let's check IntercomService for reference.
                // IntercomService: `if (isActive.HasValue) query = query.Where(i => i.IsActive == isActive.Value);`
                // And it does NOT have a default `IsActive` filter in the base query.
                // So I should remove `&& i.IsActive` from base query and only apply if `isActive.HasValue`.
                // However, `CCTVService` originally had `&& i.IsActive`.
                // I will follow IntercomService pattern: remove hardcoded `IsActive` and only filter if requested.
                // Wait, if I do that, existing calls might start seeing inactive CCTVs if they don't specify `isActive=true`.
                // The Controller defaults `isActive` to null.
                // So by default, it would show ALL (active and inactive).
                // Is this desired? "Pagination is missing... make filter option".
                // Usually "filter option" means "I want to be able to filter", not "Change default behavior".
                // But `IntercomService` shows all by default.
                // Let's stick to: If `isActive` is null, default to `true` (Active only) to be safe?
                // Or just follow the instruction "make filter option".
                // I'll implement it as: if `isActive` is null, do NOT filter (show all). This gives the most flexibility.
                // But wait, the original code was: `.Where(i => i.HasCCTV && i.IsActive)`
                // If I change it to show all by default, I might break the "view only active CCTVs" expectation.
                // Let's look at the implementation plan again.
                // "Update GetAllCCTVsAsync to accept filter parameters... Apply filters to the query."
                // I'll make `isActive` default to `true` in the Controller? No, usually null in controller.
                // Let's set the default in the Service to `true` if null?
                // `if (isActive.HasValue) ... else query = query.Where(i => i.IsActive);`
                // This preserves original behavior (only active) while allowing `isActive=false` to see inactive ones.
                
                // Actually, looking at `IntercomService`, it has `if (isActive.HasValue) ...`.
                // And the base query is `_context.Set<Intercom>()...`. It doesn't filter by IsActive by default.
                // So Intercoms list shows everything.
                // CCTV list was showing only active.
                // I will preserve the "Only Active" default behavior for CCTV list, as it might be used for a dashboard or something.
                // So:
                if (isActive.HasValue)
                {
                    query = query.Where(i => i.IsActive == isActive.Value);
                }
                else
                {
                    query = query.Where(i => i.IsActive);
                }
            }

            // Apply access control
            if (user.UserTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                var allowedBuildingIds = await GetUserBuildingIdsAsync(currentUserId, user);
                query = query.Where(i => allowedBuildingIds.Contains(i.BuildingId));
            }

            var totalCount = await query.CountAsync();

            var cctvs = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cctvs.Select(MapToDto), totalCount);
        }

        public async Task<CCTVResponseDto?> GetCCTVByIdAsync(int id, int currentUserId)
        {
            var intercom = await _context.Intercoms
                .Include(i => i.Building)
                .FirstOrDefaultAsync(i => i.IntercomId == id && i.HasCCTV);

            if (intercom == null) return null;

            if (!await HasAccessToBuildingAsync(intercom.BuildingId, currentUserId))
            {
                return null;
            }

            return MapToDto(intercom);
        }

        public async Task<CCTVResponseDto> CreateCCTVAsync(CreateCCTVDto createDto, int createdByUserId)
        {
            // Verify access to building
            if (!await HasAccessToBuildingAsync(createDto.BuildingId, createdByUserId))
            {
                throw new UnauthorizedAccessException("You do not have permission to add CCTV to this building.");
            }

            var intercom = new Intercom
            {
                IntercomName = createDto.CCTVName,
                StreamUrl = createDto.StreamUrl,
                InstalledLocation = createDto.Location,
                BuildingId = createDto.BuildingId,
                CustomerId = createDto.CustomerId ?? await GetCustomerIdFromBuilding(createDto.BuildingId),
                HasCCTV = true,
                IsActive = true,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                // Defaults for required fields
                IsInstalled = true
            };

            _context.Intercoms.Add(intercom);
            await _context.SaveChangesAsync();

            // Reload to get included data
            return await GetCCTVByIdAsync(intercom.IntercomId, createdByUserId) 
                   ?? throw new InvalidOperationException("Failed to retrieve created CCTV");
        }

        public async Task<CCTVResponseDto?> UpdateCCTVAsync(int id, UpdateCCTVDto updateDto, int currentUserId)
        {
            var intercom = await _context.Intercoms.FindAsync(id);
            if (intercom == null || !intercom.HasCCTV) return null;

            if (!await HasAccessToBuildingAsync(intercom.BuildingId, currentUserId))
            {
                return null;
            }

            if (updateDto.CCTVName != null) intercom.IntercomName = updateDto.CCTVName;
            if (updateDto.StreamUrl != null) intercom.StreamUrl = updateDto.StreamUrl;
            if (updateDto.Location != null) intercom.InstalledLocation = updateDto.Location;
            if (updateDto.IsActive.HasValue) intercom.IsActive = updateDto.IsActive.Value;

            intercom.UpdatedAt = DateTime.UtcNow;
            intercom.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            return await GetCCTVByIdAsync(id, currentUserId);
        }

        public async Task<bool> DeleteCCTVAsync(int id, int currentUserId)
        {
            var intercom = await _context.Intercoms.FindAsync(id);
            if (intercom == null || !intercom.HasCCTV) return false;

            if (!await HasAccessToBuildingAsync(intercom.BuildingId, currentUserId))
            {
                return false;
            }

            // Soft delete
            intercom.IsActive = false;
            intercom.UpdatedAt = DateTime.UtcNow;
            intercom.UpdatedBy = currentUserId;
            
            await _context.SaveChangesAsync();
            return true;
        }

        private CCTVResponseDto MapToDto(Intercom intercom)
        {
            return new CCTVResponseDto
            {
                CCTVId = intercom.IntercomId,
                CCTVName = intercom.IntercomName,
                StreamUrl = intercom.StreamUrl,
                BuildingName = intercom.Building?.Name,
                BuildingAddress = intercom.Building?.Address,
                Location = intercom.InstalledLocation,
                IsActive = intercom.IsActive,
                CreatedAt = intercom.CreatedAt
            };
        }

        private async Task<List<int>> GetUserBuildingIdsAsync(int userId, User user)
        {
            // 1. Explicit permissions
            var explicitBuildingIds = await _context.UserBuildingPermissions
                .Where(p => p.UserId == userId && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();

            if (explicitBuildingIds.Any())
            {
                return explicitBuildingIds;
            }

            // 2. PM Ecosystem
            int? pmId = null;
            if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                pmId = user.Id;
            }
            else
            {
                pmId = await FindPropertyManagerForUserAsync(user.Id);
            }

            if (pmId.HasValue)
            {
                return await _context.Buildings
                    .Where(b => b.CustomerId == pmId.Value && b.IsActive)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
            }

            return new List<int>();
        }

        private async Task<bool> HasAccessToBuildingAsync(int buildingId, int userId)
        {
            var user = await GetUserWithAccessControlAsync(userId);
            if (user == null) return false;

            if (user.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID) return true;

            var allowedBuildingIds = await GetUserBuildingIdsAsync(userId, user);
            return allowedBuildingIds.Contains(buildingId);
        }

        private async Task<int> GetCustomerIdFromBuilding(int buildingId)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            return building?.CustomerId ?? 0;
        }
    }
}

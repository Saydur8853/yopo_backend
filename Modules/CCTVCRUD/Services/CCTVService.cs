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
        Task<IEnumerable<CCTVResponseDto>> GetAllCCTVsAsync(int currentUserId);
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

        public async Task<IEnumerable<CCTVResponseDto>> GetAllCCTVsAsync(int currentUserId)
        {
            var user = await GetUserWithAccessControlAsync(currentUserId);
            if (user == null) return new List<CCTVResponseDto>();

            var query = _context.Intercoms
                .Include(i => i.Building)
                .Where(i => i.HasCCTV && i.IsActive)
                .AsQueryable();

            // Apply access control
            if (user.UserTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                var allowedBuildingIds = await GetUserBuildingIdsAsync(currentUserId, user);
                query = query.Where(i => allowedBuildingIds.Contains(i.BuildingId));
            }

            var cctvs = await query.ToListAsync();
            return cctvs.Select(MapToDto);
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

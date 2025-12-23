using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.AnnouncementCRUD.DTOs;
using YopoBackend.Modules.AnnouncementCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using Microsoft.AspNetCore.SignalR;
using YopoBackend.Hubs;
using YopoBackend.Services;

namespace YopoBackend.Modules.AnnouncementCRUD.Services
{
    public class AnnouncementService : BaseAccessControlService, IAnnouncementService
    {
        private readonly IHubContext<AnnouncementHub> _hubContext;

        public AnnouncementService(ApplicationDbContext context, IHubContext<AnnouncementHub> hubContext) : base(context)
        {
            _hubContext = hubContext;
        }

        public async Task<AnnouncementListResponseDTO> GetAnnouncementsAsync(int currentUserId, int page = 1, int pageSize = 10, int? buildingId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                return new AnnouncementListResponseDTO
                {
                    Page = page,
                    PageSize = pageSize
                };
            }

            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUser);
            if (!accessibleBuildingIds.Any())
            {
                return new AnnouncementListResponseDTO
                {
                    Page = page,
                    PageSize = pageSize
                };
            }

            var query = _context.Announcements.AsNoTracking().Where(a => accessibleBuildingIds.Contains(a.BuildingId));

            if (buildingId.HasValue)
            {
                if (!accessibleBuildingIds.Contains(buildingId.Value))
                {
                    return new AnnouncementListResponseDTO
                    {
                        Page = page,
                        PageSize = pageSize
                    };
                }

                query = query.Where(a => a.BuildingId == buildingId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Join(_context.Users.AsNoTracking(),
                    a => a.CreatedBy,
                    u => u.Id,
                    (a, u) => new { Announcement = a, CreatorName = u.Name })
                .OrderByDescending(x => x.Announcement.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AnnouncementResponseDTO
                {
                    AnnouncementId = x.Announcement.AnnouncementId,
                    BuildingId = x.Announcement.BuildingId,
                    Subject = x.Announcement.Subject,
                    Body = x.Announcement.Body,
                    AnnouncementDate = x.Announcement.AnnouncementDate,
                    AnnouncementTime = x.Announcement.AnnouncementTime,
                    CreatedBy = x.Announcement.CreatedBy,
                    CreatedByName = x.CreatorName,
                    CreatedAt = x.Announcement.CreatedAt,
                    UpdatedAt = x.Announcement.UpdatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new AnnouncementListResponseDTO
            {
                Announcements = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }

        public async Task<AnnouncementResponseDTO> CreateAnnouncementAsync(CreateAnnouncementDTO dto, int currentUserId)
        {
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User authentication required.");
            }

            if (!IsAnnouncementPublisher(currentUser))
            {
                throw new UnauthorizedAccessException("Only Property Managers can create announcements.");
            }

            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUser);
            if (!accessibleBuildingIds.Contains(dto.BuildingId))
            {
                throw new UnauthorizedAccessException("Access denied to the specified building.");
            }

            var announcement = new Announcement
            {
                BuildingId = dto.BuildingId,
                Subject = dto.Subject,
                Body = dto.Body,
                AnnouncementDate = dto.AnnouncementDate,
                AnnouncementTime = dto.AnnouncementTime,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var response = new AnnouncementResponseDTO
            {
                AnnouncementId = announcement.AnnouncementId,
                BuildingId = announcement.BuildingId,
                Subject = announcement.Subject,
                Body = announcement.Body,
                AnnouncementDate = announcement.AnnouncementDate,
                AnnouncementTime = announcement.AnnouncementTime,
                CreatedBy = announcement.CreatedBy,
                CreatedByName = currentUser.Name,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            await _hubContext.Clients.Group(BuildingGroup(announcement.BuildingId))
                .SendAsync("AnnouncementCreated", response);

            return response;
        }

        public async Task<AnnouncementResponseDTO?> UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementDTO dto, int currentUserId)
        {
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User authentication required.");
            }

            if (!IsAnnouncementPublisher(currentUser))
            {
                throw new UnauthorizedAccessException("Only Property Managers can edit announcements.");
            }

            var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
            if (announcement == null)
            {
                return null;
            }

            if (!await HasAccessToEntityAsync(announcement, currentUserId))
            {
                throw new UnauthorizedAccessException("Access denied to this announcement.");
            }

            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUser);
            if (!accessibleBuildingIds.Contains(announcement.BuildingId))
            {
                throw new UnauthorizedAccessException("Access denied to the specified building.");
            }

            if (dto.Subject != null) announcement.Subject = dto.Subject;
            if (dto.Body != null) announcement.Body = dto.Body;
            if (dto.AnnouncementDate.HasValue) announcement.AnnouncementDate = dto.AnnouncementDate;
            if (dto.AnnouncementTime != null) announcement.AnnouncementTime = dto.AnnouncementTime;

            announcement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var creatorName = await _context.Users
                .Where(u => u.Id == announcement.CreatedBy)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            var response = new AnnouncementResponseDTO
            {
                AnnouncementId = announcement.AnnouncementId,
                BuildingId = announcement.BuildingId,
                Subject = announcement.Subject,
                Body = announcement.Body,
                AnnouncementDate = announcement.AnnouncementDate,
                AnnouncementTime = announcement.AnnouncementTime,
                CreatedBy = announcement.CreatedBy,
                CreatedByName = creatorName,
                CreatedAt = announcement.CreatedAt,
                UpdatedAt = announcement.UpdatedAt
            };

            await _hubContext.Clients.Group(BuildingGroup(announcement.BuildingId))
                .SendAsync("AnnouncementUpdated", response);

            return response;
        }

        public async Task<bool> DeleteAnnouncementAsync(int announcementId, int currentUserId)
        {
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User authentication required.");
            }

            if (!IsAnnouncementPublisher(currentUser))
            {
                throw new UnauthorizedAccessException("Only Property Managers can delete announcements.");
            }

            var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
            if (announcement == null)
            {
                return false;
            }

            if (!await HasAccessToEntityAsync(announcement, currentUserId))
            {
                throw new UnauthorizedAccessException("Access denied to this announcement.");
            }

            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUser);
            if (!accessibleBuildingIds.Contains(announcement.BuildingId))
            {
                throw new UnauthorizedAccessException("Access denied to the specified building.");
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(BuildingGroup(announcement.BuildingId))
                .SendAsync("AnnouncementDeleted", new { announcementId });

            return true;
        }

        private bool IsAnnouncementPublisher(User currentUser)
        {
            return currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID ||
                   currentUser.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID;
        }

        private async Task<List<int>> GetAccessibleBuildingIdsAsync(User currentUser)
        {
            if (currentUser.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return await _context.Buildings.Select(b => b.BuildingId).ToListAsync();
            }

            var explicitBuildingIds = await _context.UserBuildingPermissions
                .Where(p => p.UserId == currentUser.Id && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();

            if (explicitBuildingIds.Any())
            {
                return explicitBuildingIds;
            }

            if (currentUser.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                var tenantBuildingIds = await _context.Tenants
                    .Where(t => t.TenantId == currentUser.Id)
                    .Select(t => t.BuildingId)
                    .ToListAsync();

                var unitBuildingIds = await _context.Units
                    .Where(u => u.TenantId == currentUser.Id)
                    .Select(u => u.BuildingId)
                    .ToListAsync();

                return tenantBuildingIds.Concat(unitBuildingIds).Distinct().ToList();
            }

            if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                var pmId = await FindPropertyManagerForUserAsync(currentUser.Id) ?? currentUser.Id;
                return await _context.Buildings
                    .Where(b => b.CustomerId == pmId)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
            }

            if (currentUser.InviteById.HasValue)
            {
                return new List<int>();
            }

            return new List<int>();
        }

        private static string BuildingGroup(int buildingId) => $"Building_{buildingId}";
    }
}

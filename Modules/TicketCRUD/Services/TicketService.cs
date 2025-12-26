using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Hubs;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.TicketCRUD.DTOs;
using YopoBackend.Modules.TicketCRUD.Models;
using YopoBackend.Modules.UnitCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.TicketCRUD.Services
{
    public class TicketService : BaseAccessControlService, ITicketService
    {
        private readonly IHubContext<TicketHub> _hubContext;

        private static readonly Dictionary<string, string> StatusMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { TicketStatus.New, TicketStatus.New },
            { TicketStatus.Accepted, TicketStatus.Accepted },
            { TicketStatus.Investigating, TicketStatus.Investigating },
            { TicketStatus.TimeFrameSet, TicketStatus.TimeFrameSet },
            { TicketStatus.ServiceManSent, TicketStatus.ServiceManSent },
            { TicketStatus.Feedback, TicketStatus.Feedback },
            { TicketStatus.Done, TicketStatus.Done },
            { TicketStatus.Rejected, TicketStatus.Rejected }
        };

        private static readonly Dictionary<string, string> ConcernMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { TicketConcernLevel.Low, TicketConcernLevel.Low },
            { TicketConcernLevel.Normal, TicketConcernLevel.Normal },
            { TicketConcernLevel.Medium, TicketConcernLevel.Medium },
            { TicketConcernLevel.High, TicketConcernLevel.High },
            { TicketConcernLevel.Urgent, TicketConcernLevel.Urgent }
        };

        public TicketService(ApplicationDbContext context, IHubContext<TicketHub> hubContext) : base(context)
        {
            _hubContext = hubContext;
        }

        public async Task<(List<TicketResponseDTO> tickets, int totalRecords)> GetTicketsAsync(
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            int? buildingId = null,
            int? unitId = null,
            string? status = null,
            string? concernLevel = null,
            string? searchTerm = null,
            bool includeDeleted = false)
        {
            var query = _context.Set<Ticket>()
                .Include(t => t.Building)
                .Include(t => t.Unit)
                .Include(t => t.TenantUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            query = await ApplyTicketAccessControlAsync(query, currentUserId);

            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser?.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                var tenantUnit = await _context.Units
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.TenantId == currentUserId);
                if (tenantUnit == null)
                {
                    return (new List<TicketResponseDTO>(), 0);
                }

                if (buildingId.HasValue && buildingId.Value != tenantUnit.BuildingId)
                {
                    return (new List<TicketResponseDTO>(), 0);
                }

                buildingId = tenantUnit.BuildingId;
                unitId = tenantUnit.UnitId;
            }
            else
            {
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == currentUserId && p.IsActive)
                    .Select(p => p.BuildingId)
                    .ToListAsync();
                if (explicitBuildingIds.Any())
                {
                    query = query.Where(t => explicitBuildingIds.Contains(t.BuildingId));
                }
            }

            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }

            if (buildingId.HasValue)
            {
                query = query.Where(t => t.BuildingId == buildingId.Value);
            }

            if (unitId.HasValue)
            {
                query = query.Where(t => t.UnitId == unitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (StatusMap.TryGetValue(status, out var normalizedStatus))
                {
                    query = query.Where(t => t.Status == normalizedStatus);
                }
                else
                {
                    return (new List<TicketResponseDTO>(), 0);
                }
            }

            if (!string.IsNullOrWhiteSpace(concernLevel))
            {
                if (ConcernMap.TryGetValue(concernLevel, out var normalizedConcern))
                {
                    query = query.Where(t => t.ConcernLevel == normalizedConcern);
                }
                else
                {
                    return (new List<TicketResponseDTO>(), 0);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm) ||
                    (t.Unit != null && t.Unit.UnitNumber.Contains(searchTerm)) ||
                    t.Building.Name.Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedTickets = tickets.Select(MapToResponse).ToList();
            return (mappedTickets, totalRecords);
        }

        public async Task<TicketResponseDTO> CreateTicketAsync(CreateTicketDTO dto, int currentUserId)
        {
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User authentication required.");
            }

            if (currentUser.UserTypeId != UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                throw new UnauthorizedAccessException("Only tenants can create tickets.");
            }

            var tenantUnit = await _context.Units.FirstOrDefaultAsync(u => u.TenantId == currentUserId);
            if (tenantUnit == null)
            {
                throw new InvalidOperationException("Tenant has no allocated unit.");
            }

            var buildingId = tenantUnit.BuildingId;
            var unitId = tenantUnit.UnitId;
            var tenantUserId = currentUserId;

            var concern = NormalizeConcern(dto.ConcernLevel);

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                BuildingId = buildingId,
                UnitId = unitId,
                TenantUserId = tenantUserId,
                ConcernLevel = concern ?? TicketConcernLevel.Normal,
                Status = TicketStatus.New,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<Ticket>().Add(ticket);
            await _context.SaveChangesAsync();

            var created = await _context.Set<Ticket>()
                .Include(t => t.Building)
                .Include(t => t.Unit)
                .Include(t => t.TenantUser)
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.TicketId == ticket.TicketId);

            var response = MapToResponse(created);
            await NotifyTicketCreatedAsync(response);
            return response;
        }

        public async Task<TicketMutationResult> UpdateTicketAsync(int ticketId, UpdateTicketDTO dto, int currentUserId)
        {
            var query = _context.Set<Ticket>().Where(t => t.TicketId == ticketId && !t.IsDeleted);
            query = await ApplyTicketAccessControlAsync(query, currentUserId);

            var ticket = await query.FirstOrDefaultAsync();
            if (ticket == null)
            {
                return new TicketMutationResult { NotFound = true };
            }

            if (IsFinalStatus(ticket.Status))
            {
                return new TicketMutationResult { Locked = true, Message = "Ticket is locked." };
            }

            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser?.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                if (!string.IsNullOrWhiteSpace(dto.Status) ||
                    !string.IsNullOrWhiteSpace(dto.TimeFrame) ||
                    !string.IsNullOrWhiteSpace(dto.ServicePerson) ||
                    !string.IsNullOrWhiteSpace(dto.Feedback) ||
                    !string.IsNullOrWhiteSpace(dto.RejectionReason))
                {
                    return new TicketMutationResult { Locked = true, Message = "Tenant cannot update workflow fields." };
                }
            }

            var statusChanged = false;
            var previousStatus = ticket.Status;

            if (dto.Title != null) ticket.Title = dto.Title;
            if (dto.Description != null) ticket.Description = dto.Description;
            if (dto.UnitId.HasValue)
            {
                await ValidateUnitAsync(ticket.BuildingId, dto.UnitId);
                ticket.UnitId = dto.UnitId;
            }

            var normalizedConcern = NormalizeConcern(dto.ConcernLevel);
            if (normalizedConcern != null)
            {
                ticket.ConcernLevel = normalizedConcern;
            }

            var normalizedStatus = NormalizeStatus(dto.Status);
            if (normalizedStatus != null && normalizedStatus != ticket.Status)
            {
                ticket.Status = normalizedStatus;
                ticket.StatusUpdatedAt = DateTime.UtcNow;
                statusChanged = !string.Equals(previousStatus, ticket.Status, StringComparison.OrdinalIgnoreCase);
            }

            if (dto.TimeFrame != null) ticket.TimeFrame = dto.TimeFrame;
            if (dto.ServicePerson != null) ticket.ServicePerson = dto.ServicePerson;
            if (dto.Feedback != null) ticket.Feedback = dto.Feedback;
            if (dto.RejectionReason != null) ticket.RejectionReason = dto.RejectionReason;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.Set<Ticket>()
                .Include(t => t.Building)
                .Include(t => t.Unit)
                .Include(t => t.TenantUser)
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.TicketId == ticket.TicketId);

            var response = MapToResponse(updated);
            await NotifyTicketUpdatedAsync(response);
            if (statusChanged)
            {
                await NotifyTicketStatusChangedAsync(response);
            }
            return new TicketMutationResult { Ticket = response };
        }

        public async Task<TicketMutationResult> DeleteTicketAsync(int ticketId, int currentUserId)
        {
            var query = _context.Set<Ticket>().Where(t => t.TicketId == ticketId && !t.IsDeleted);
            query = await ApplyTicketAccessControlAsync(query, currentUserId);

            var ticket = await query.FirstOrDefaultAsync();
            if (ticket == null)
            {
                return new TicketMutationResult { NotFound = true };
            }

            if (IsFinalStatus(ticket.Status))
            {
                return new TicketMutationResult { Locked = true, Message = "Ticket is locked." };
            }

            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;
            ticket.DeletedBy = currentUserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var deleted = await _context.Set<Ticket>()
                .Include(t => t.Building)
                .Include(t => t.Unit)
                .Include(t => t.TenantUser)
                .Include(t => t.CreatedByUser)
                .FirstAsync(t => t.TicketId == ticket.TicketId);

            var response = MapToResponse(deleted);
            await NotifyTicketDeletedAsync(response);
            return new TicketMutationResult { Ticket = response };
        }

        private async Task<IQueryable<Ticket>> ApplyTicketAccessControlAsync(IQueryable<Ticket> query, int currentUserId)
        {
            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                return query.Where(t => false);
            }

            if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                var pmEcosystemIds = await GetPMEcosystemUserIdsAsync(currentUserId);
                return query.Where(t =>
                    pmEcosystemIds.Contains(t.CreatedBy) ||
                    (t.TenantUserId.HasValue && pmEcosystemIds.Contains(t.TenantUserId.Value)));
            }

            return await ApplyAccessControlAsync(query, currentUserId);
        }

        private static bool IsFinalStatus(string status)
        {
            return string.Equals(status, TicketStatus.Done, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, TicketStatus.Rejected, StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            if (!StatusMap.TryGetValue(status.Trim(), out var normalized))
            {
                throw new ArgumentException("Invalid status value.");
            }

            return normalized;
        }

        private static string? NormalizeConcern(string? concern)
        {
            if (string.IsNullOrWhiteSpace(concern))
            {
                return null;
            }

            if (!ConcernMap.TryGetValue(concern.Trim(), out var normalized))
            {
                throw new ArgumentException("Invalid concern level.");
            }

            return normalized;
        }

        private async Task ValidateBuildingAccessAsync(int buildingId, int currentUserId)
        {
            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.BuildingId == buildingId);
            if (building == null)
            {
                throw new ArgumentException("Invalid BuildingId.");
            }

            var currentUser = await GetUserWithAccessControlAsync(currentUserId);
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User authentication required.");
            }

            if (currentUser.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                if (building.CustomerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Access denied for the specified building.");
                }
                return;
            }

            var explicitBuildingIds = await _context.UserBuildingPermissions
                .Where(p => p.UserId == currentUserId && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();

            if (explicitBuildingIds.Any() && !explicitBuildingIds.Contains(buildingId))
            {
                throw new UnauthorizedAccessException("Access denied for the specified building.");
            }
        }

        private async Task ValidateUnitAsync(int buildingId, int? unitId)
        {
            if (!unitId.HasValue)
            {
                return;
            }

            var unit = await _context.Units.AsNoTracking().FirstOrDefaultAsync(u => u.UnitId == unitId.Value);
            if (unit == null || unit.BuildingId != buildingId)
            {
                throw new ArgumentException("UnitId does not belong to the specified building.");
            }
        }

        private static TicketResponseDTO MapToResponse(Ticket ticket)
        {
            var isLocked = IsFinalStatus(ticket.Status);
            return new TicketResponseDTO
            {
                TicketId = ticket.TicketId,
                TicketCode = $"#T-{ticket.TicketId}",
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                ConcernLevel = ticket.ConcernLevel,
                TimeFrame = ticket.TimeFrame,
                ServicePerson = ticket.ServicePerson,
                Feedback = ticket.Feedback,
                RejectionReason = ticket.RejectionReason,
                BuildingId = ticket.BuildingId,
                BuildingName = ticket.Building?.Name ?? string.Empty,
                UnitId = ticket.UnitId,
                UnitNumber = ticket.Unit?.UnitNumber,
                TenantUserId = ticket.TenantUserId,
                TenantName = ticket.TenantUser?.Name,
                CreatedById = ticket.CreatedBy,
                CreatedByName = ticket.CreatedByUser?.Name ?? string.Empty,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                StatusUpdatedAt = ticket.StatusUpdatedAt,
                IsDeleted = ticket.IsDeleted,
                IsLocked = isLocked
            };
        }

        private async Task NotifyTicketCreatedAsync(TicketResponseDTO response)
        {
            if (response.BuildingId <= 0)
            {
                return;
            }

            var recipientIds = await GetTicketNotificationUserIdsAsync(response.BuildingId, response.TenantUserId);
            if (recipientIds.Count == 0)
            {
                return;
            }

            var tasks = recipientIds
                .Select(userId => _hubContext.Clients.Group(TicketHub.UserGroup(userId))
                    .SendAsync("TicketCreated", response));

            await Task.WhenAll(tasks);
        }

        private async Task NotifyTicketStatusChangedAsync(TicketResponseDTO response)
        {
            if (response.BuildingId <= 0)
            {
                return;
            }

            var recipientIds = await GetTicketNotificationUserIdsAsync(response.BuildingId, response.TenantUserId);
            if (recipientIds.Count == 0)
            {
                return;
            }

            var payload = new TicketStatusChangedDTO
            {
                TicketId = response.TicketId,
                BuildingId = response.BuildingId,
                Status = response.Status,
                StatusUpdatedAt = response.StatusUpdatedAt
            };

            var tasks = recipientIds
                .Select(userId => _hubContext.Clients.Group(TicketHub.UserGroup(userId))
                    .SendAsync("TicketStatusChanged", payload));

            await Task.WhenAll(tasks);
        }

        private async Task NotifyTicketUpdatedAsync(TicketResponseDTO response)
        {
            if (response.BuildingId <= 0)
            {
                return;
            }

            var recipientIds = await GetTicketNotificationUserIdsAsync(response.BuildingId, response.TenantUserId);
            if (recipientIds.Count == 0)
            {
                return;
            }

            var tasks = recipientIds
                .Select(userId => _hubContext.Clients.Group(TicketHub.UserGroup(userId))
                    .SendAsync("TicketUpdated", response));

            await Task.WhenAll(tasks);
        }

        private async Task NotifyTicketDeletedAsync(TicketResponseDTO response)
        {
            if (response.BuildingId <= 0 || response.TicketId <= 0)
            {
                return;
            }

            var recipientIds = await GetTicketNotificationUserIdsAsync(response.BuildingId, response.TenantUserId);
            if (recipientIds.Count == 0)
            {
                return;
            }

            var tasks = recipientIds
                .Select(userId => _hubContext.Clients.Group(TicketHub.UserGroup(userId))
                    .SendAsync("TicketDeleted", response));

            await Task.WhenAll(tasks);
        }

        private async Task<List<int>> GetTicketNotificationUserIdsAsync(int buildingId, int? tenantUserId)
        {
            var building = await _context.Buildings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId);
            if (building == null)
            {
                return new List<int>();
            }

            var pmId = building.CustomerId;

            var ecosystemUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && (u.Id == pmId || u.InviteById == pmId || u.CreatedBy == pmId))
                .Select(u => new { u.Id, u.UserTypeId })
                .ToListAsync();

            var nonTenantIds = ecosystemUsers
                .Where(u => u.UserTypeId != UserTypeConstants.TENANT_USER_TYPE_ID)
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            if (nonTenantIds.Count == 0)
            {
                return new List<int>();
            }

            var permissions = await _context.UserBuildingPermissions
                .Where(p => nonTenantIds.Contains(p.UserId) && p.IsActive)
                .Select(p => new { p.UserId, p.BuildingId })
                .ToListAsync();

            var usersWithAnyPermissions = permissions
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            var allowedByBuilding = permissions
                .Where(p => p.BuildingId == buildingId)
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            var withoutPermissions = nonTenantIds
                .Except(usersWithAnyPermissions);

            var recipients = withoutPermissions
                .Concat(allowedByBuilding)
                .Distinct()
                .ToList();

            if (tenantUserId.HasValue && tenantUserId.Value > 0)
            {
                recipients.Add(tenantUserId.Value);
            }

            return recipients.Distinct().ToList();
        }
    }
}

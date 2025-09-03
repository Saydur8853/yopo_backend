using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Modules.NotificationCRUD.DTOs;
using YopoBackend.Modules.NotificationCRUD.Models;

namespace YopoBackend.Modules.NotificationCRUD.Services
{
    /// <summary>
    /// Service for managing notification operations.
    /// Provides business logic for notification CRUD operations and notification-specific functionality.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the NotificationService.
        /// </summary>
        /// <param name="context">The database context.</param>
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<NotificationListResponseDTO> GetAllNotificationsAsync(
            int page = 1, 
            int pageSize = 10,
            int? buildingId = null,
            int? customerId = null,
            int? tenantId = null,
            string? type = null,
            string? status = null,
            string? warningLevel = null,
            bool? isUrgent = null,
            string? searchTerm = null)
        {
            var query = _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive);

            // Apply filters
            if (buildingId.HasValue)
                query = query.Where(n => n.BuildingId == buildingId.Value);
            
            if (customerId.HasValue)
                query = query.Where(n => n.CustomerId == customerId.Value);
                
            if (tenantId.HasValue)
                query = query.Where(n => n.TenantId == tenantId.Value);
                
            if (!string.IsNullOrEmpty(type))
                query = query.Where(n => n.Type == type);
                
            if (!string.IsNullOrEmpty(status))
                query = query.Where(n => n.Status == status);
                
            if (!string.IsNullOrEmpty(warningLevel))
                query = query.Where(n => n.WarningLevel == warningLevel);
                
            if (isUrgent.HasValue)
                query = query.Where(n => n.IsUrgent == isUrgent.Value);
                
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(n => n.Title.Contains(searchTerm) || n.Description.Contains(searchTerm));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => MapToResponseDTO(n))
                .ToListAsync();

            return new NotificationListResponseDTO
            {
                Notifications = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <inheritdoc/>
        public async Task<NotificationResponseDTO?> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            return notification != null ? MapToResponseDTO(notification) : null;
        }

        /// <inheritdoc/>
        public async Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationDTO createDto, int createdBy)
        {
            var notification = new Notification
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Type = createDto.Type,
                SentTo = createDto.SentTo,
                SendFrom = createDto.SendFrom,
                Status = createDto.Status,
                BuildingId = createDto.BuildingId,
                CustomerId = createDto.CustomerId,
                TenantId = createDto.TenantId,
                WarningLevel = createDto.WarningLevel,
                Priority = createDto.Priority,
                Category = createDto.Category,
                File = createDto.File,
                ScheduledAt = createDto.ScheduledAt,
                ExpiresAt = createDto.ExpiresAt,
                RequiresAcknowledgment = createDto.RequiresAcknowledgment,
                IsUrgent = createDto.IsUrgent,
                SendEmail = createDto.SendEmail,
                SendSMS = createDto.SendSMS,
                SendPush = createDto.SendPush,
                SendInApp = createDto.SendInApp,
                Metadata = createDto.Metadata,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return await GetNotificationByIdAsync(notification.NotificationId) ?? throw new InvalidOperationException("Failed to retrieve created notification.");
        }

        /// <inheritdoc/>
        public async Task<NotificationResponseDTO?> UpdateNotificationAsync(int id, UpdateNotificationDTO updateDto)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return null;

            // Update properties
            notification.Title = updateDto.Title;
            notification.Description = updateDto.Description;
            notification.Type = updateDto.Type;
            notification.SentTo = updateDto.SentTo;
            notification.SendFrom = updateDto.SendFrom;
            notification.Status = updateDto.Status;
            notification.BuildingId = updateDto.BuildingId;
            notification.CustomerId = updateDto.CustomerId;
            notification.TenantId = updateDto.TenantId;
            notification.WarningLevel = updateDto.WarningLevel;
            notification.Priority = updateDto.Priority;
            notification.Category = updateDto.Category;
            notification.File = updateDto.File;
            notification.ScheduledAt = updateDto.ScheduledAt;
            notification.ExpiresAt = updateDto.ExpiresAt;
            notification.RequiresAcknowledgment = updateDto.RequiresAcknowledgment;
            notification.IsUrgent = updateDto.IsUrgent;
            notification.SendEmail = updateDto.SendEmail;
            notification.SendSMS = updateDto.SendSMS;
            notification.SendPush = updateDto.SendPush;
            notification.SendInApp = updateDto.SendInApp;
            notification.Metadata = updateDto.Metadata;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetNotificationByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<NotificationResponseDTO?> UpdateNotificationStatusAsync(int id, UpdateNotificationStatusDTO statusDto)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return null;

            notification.Status = statusDto.Status;
            notification.UpdatedAt = statusDto.StatusUpdatedAt ?? DateTime.UtcNow;

            // Update status-specific timestamps
            switch (statusDto.Status.ToLower())
            {
                case "sent":
                    notification.SentAt = DateTime.UtcNow;
                    break;
                case "delivered":
                    notification.DeliveredAt = DateTime.UtcNow;
                    break;
                case "read":
                    notification.ReadAt = DateTime.UtcNow;
                    notification.ReadCount++;
                    break;
            }

            await _context.SaveChangesAsync();

            return await GetNotificationByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return false;

            notification.IsActive = false;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> PermanentDeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> SendNotificationAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return false;

            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> MarkNotificationAsReadAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return false;

            if (notification.ReadAt == null)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.ReadCount++;
                notification.Status = "Read";
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<NotificationListResponseDTO> GetNotificationsByBuildingAsync(int buildingId, int page = 1, int pageSize = 10, bool includeRead = true)
        {
            var query = _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive && n.BuildingId == buildingId);

            if (!includeRead)
                query = query.Where(n => n.ReadAt == null);

            return await ExecutePaginatedQuery(query, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<NotificationListResponseDTO> GetNotificationsByCustomerAsync(int customerId, int page = 1, int pageSize = 10, bool includeRead = true)
        {
            var query = _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive && n.CustomerId == customerId);

            if (!includeRead)
                query = query.Where(n => n.ReadAt == null);

            return await ExecutePaginatedQuery(query, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<NotificationListResponseDTO> GetNotificationsByTenantAsync(int tenantId, int page = 1, int pageSize = 10, bool includeRead = true)
        {
            var query = _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive && n.TenantId == tenantId);

            if (!includeRead)
                query = query.Where(n => n.ReadAt == null);

            return await ExecutePaginatedQuery(query, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<NotificationListResponseDTO> GetUrgentNotificationsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive && (n.IsUrgent || n.Priority <= 2));

            return await ExecutePaginatedQuery(query, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<object> GetNotificationStatisticsAsync(int? buildingId = null, int? customerId = null, int? tenantId = null)
        {
            var query = _context.Notifications.Where(n => n.IsActive);

            if (buildingId.HasValue)
                query = query.Where(n => n.BuildingId == buildingId.Value);
            if (customerId.HasValue)
                query = query.Where(n => n.CustomerId == customerId.Value);
            if (tenantId.HasValue)
                query = query.Where(n => n.TenantId == tenantId.Value);

            var stats = await query
                .GroupBy(n => 1)
                .Select(g => new
                {
                    TotalNotifications = g.Count(),
                    UnreadNotifications = g.Count(n => n.ReadAt == null),
                    ReadNotifications = g.Count(n => n.ReadAt != null),
                    UrgentNotifications = g.Count(n => n.IsUrgent),
                    SentNotifications = g.Count(n => n.Status == "Sent"),
                    PendingNotifications = g.Count(n => n.Status == "Draft" || n.Status == "Scheduled"),
                    ByType = g.GroupBy(n => n.Type)
                        .Select(tg => new { Type = tg.Key, Count = tg.Count() })
                        .ToList(),
                    ByWarningLevel = g.Where(n => n.WarningLevel != null)
                        .GroupBy(n => n.WarningLevel)
                        .Select(wg => new { WarningLevel = wg.Key, Count = wg.Count() })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                return new
                {
                    TotalNotifications = 0,
                    UnreadNotifications = 0,
                    ReadNotifications = 0,
                    UrgentNotifications = 0,
                    SentNotifications = 0,
                    PendingNotifications = 0,
                    ByType = new List<object>(),
                    ByWarningLevel = new List<object>()
                };
            }

            return stats;
        }

        /// <inheritdoc/>
        public async Task<bool> ScheduleNotificationAsync(int id, DateTime scheduledAt)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.IsActive);

            if (notification == null)
                return false;

            notification.ScheduledAt = scheduledAt;
            notification.Status = "Scheduled";
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<NotificationResponseDTO>> GetScheduledNotificationsReadyToSendAsync()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Building)
                .Include(n => n.Customer)
                .Include(n => n.Tenant)
                .Include(n => n.Creator)
                .Where(n => n.IsActive && 
                           n.Status == "Scheduled" && 
                           n.ScheduledAt.HasValue && 
                           n.ScheduledAt.Value <= DateTime.UtcNow)
                .Select(n => MapToResponseDTO(n))
                .ToListAsync();

            return notifications;
        }

        /// <summary>
        /// Executes a paginated query and returns the result.
        /// </summary>
        private async Task<NotificationListResponseDTO> ExecutePaginatedQuery(IQueryable<Notification> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => MapToResponseDTO(n))
                .ToListAsync();

            return new NotificationListResponseDTO
            {
                Notifications = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Maps a Notification entity to a NotificationResponseDTO.
        /// </summary>
        private static NotificationResponseDTO MapToResponseDTO(Notification notification)
        {
            return new NotificationResponseDTO
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Description = notification.Description,
                Type = notification.Type,
                SentTo = notification.SentTo,
                SendFrom = notification.SendFrom,
                Status = notification.Status,
                BuildingId = notification.BuildingId,
                BuildingName = notification.Building?.Name,
                CustomerId = notification.CustomerId,
                CustomerName = notification.Customer?.Name,
                TenantId = notification.TenantId,
                TenantName = notification.Tenant?.Name,
                WarningLevel = notification.WarningLevel,
                Priority = notification.Priority,
                Category = notification.Category,
                File = notification.File,
                ScheduledAt = notification.ScheduledAt,
                SentAt = notification.SentAt,
                DeliveredAt = notification.DeliveredAt,
                ReadAt = notification.ReadAt,
                ExpiresAt = notification.ExpiresAt,
                RequiresAcknowledgment = notification.RequiresAcknowledgment,
                IsUrgent = notification.IsUrgent,
                SendEmail = notification.SendEmail,
                SendSMS = notification.SendSMS,
                SendPush = notification.SendPush,
                SendInApp = notification.SendInApp,
                Metadata = notification.Metadata,
                RecipientCount = notification.RecipientCount,
                ReadCount = notification.ReadCount,
                IsActive = notification.IsActive,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt,
                CreatedBy = notification.CreatedBy,
                CreatedByName = notification.Creator != null ? $"{notification.Creator.FirstName} {notification.Creator.LastName}".Trim() : null
            };
        }
    }
}

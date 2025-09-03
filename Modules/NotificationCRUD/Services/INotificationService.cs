using YopoBackend.Modules.NotificationCRUD.DTOs;
using YopoBackend.Modules.NotificationCRUD.Models;

namespace YopoBackend.Modules.NotificationCRUD.Services
{
    /// <summary>
    /// Interface for notification service operations.
    /// Provides methods for managing notifications including CRUD operations and notification-specific functionality.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Retrieves all notifications with optional pagination and filtering.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="buildingId">Optional building ID filter.</param>
        /// <param name="customerId">Optional customer ID filter.</param>
        /// <param name="tenantId">Optional tenant ID filter.</param>
        /// <param name="type">Optional notification type filter.</param>
        /// <param name="status">Optional status filter.</param>
        /// <param name="warningLevel">Optional warning level filter.</param>
        /// <param name="isUrgent">Optional filter for urgent notifications.</param>
        /// <param name="searchTerm">Optional search term for title and description.</param>
        /// <returns>A paginated list of notifications.</returns>
        Task<NotificationListResponseDTO> GetAllNotificationsAsync(
            int page = 1, 
            int pageSize = 10,
            int? buildingId = null,
            int? customerId = null,
            int? tenantId = null,
            string? type = null,
            string? status = null,
            string? warningLevel = null,
            bool? isUrgent = null,
            string? searchTerm = null);

        /// <summary>
        /// Retrieves a notification by its ID.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>The notification if found, null otherwise.</returns>
        Task<NotificationResponseDTO?> GetNotificationByIdAsync(int id);

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="createDto">The notification data.</param>
        /// <param name="createdBy">The ID of the user creating the notification.</param>
        /// <returns>The created notification.</returns>
        Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationDTO createDto, int createdBy);

        /// <summary>
        /// Updates an existing notification.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="updateDto">The updated notification data.</param>
        /// <returns>The updated notification if found and updated successfully, null otherwise.</returns>
        Task<NotificationResponseDTO?> UpdateNotificationAsync(int id, UpdateNotificationDTO updateDto);

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="statusDto">The status update data.</param>
        /// <returns>The updated notification if found and updated successfully, null otherwise.</returns>
        Task<NotificationResponseDTO?> UpdateNotificationStatusAsync(int id, UpdateNotificationStatusDTO statusDto);

        /// <summary>
        /// Soft deletes a notification by setting IsActive to false.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteNotificationAsync(int id);

        /// <summary>
        /// Permanently deletes a notification from the database.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> PermanentDeleteNotificationAsync(int id);

        /// <summary>
        /// Sends a notification (updates status and timestamps).
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>True if sent successfully, false otherwise.</returns>
        Task<bool> SendNotificationAsync(int id);

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>True if marked as read successfully, false otherwise.</returns>
        Task<bool> MarkNotificationAsReadAsync(int id);

        /// <summary>
        /// Gets notifications for a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the building.</returns>
        Task<NotificationListResponseDTO> GetNotificationsByBuildingAsync(int buildingId, int page = 1, int pageSize = 10, bool includeRead = true);

        /// <summary>
        /// Gets notifications for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the customer.</returns>
        Task<NotificationListResponseDTO> GetNotificationsByCustomerAsync(int customerId, int page = 1, int pageSize = 10, bool includeRead = true);

        /// <summary>
        /// Gets notifications for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the tenant.</returns>
        Task<NotificationListResponseDTO> GetNotificationsByTenantAsync(int tenantId, int page = 1, int pageSize = 10, bool includeRead = true);

        /// <summary>
        /// Gets urgent/high-priority notifications.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of urgent notifications.</returns>
        Task<NotificationListResponseDTO> GetUrgentNotificationsAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Gets notification statistics.
        /// </summary>
        /// <param name="buildingId">Optional building ID filter.</param>
        /// <param name="customerId">Optional customer ID filter.</param>
        /// <param name="tenantId">Optional tenant ID filter.</param>
        /// <returns>Notification statistics.</returns>
        Task<object> GetNotificationStatisticsAsync(int? buildingId = null, int? customerId = null, int? tenantId = null);

        /// <summary>
        /// Schedules a notification for future sending.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="scheduledAt">When to send the notification.</param>
        /// <returns>True if scheduled successfully, false otherwise.</returns>
        Task<bool> ScheduleNotificationAsync(int id, DateTime scheduledAt);

        /// <summary>
        /// Gets scheduled notifications that are ready to be sent.
        /// </summary>
        /// <returns>List of notifications ready to be sent.</returns>
        Task<List<NotificationResponseDTO>> GetScheduledNotificationsReadyToSendAsync();
    }
}

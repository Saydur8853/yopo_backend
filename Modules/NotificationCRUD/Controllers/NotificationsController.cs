using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.NotificationCRUD.DTOs;
using YopoBackend.Modules.NotificationCRUD.Services;

namespace YopoBackend.Modules.NotificationCRUD.Controllers
{
    /// <summary>
    /// Controller for managing notification operations.
    /// Module ID: 14 - NotificationCRUD Module
    /// Provides REST API endpoints for notification CRUD operations and notification-specific functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [RequireModule(ModuleConstants.NOTIFICATION_CRUD)]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the NotificationsController.
        /// </summary>
        /// <param name="notificationService">The notification service.</param>
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets all notifications with optional filtering and pagination.
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
        [HttpGet]
        public async Task<ActionResult<NotificationListResponseDTO>> GetAllNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] string? warningLevel = null,
            [FromQuery] bool? isUrgent = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                if (pageSize > 100)
                {
                    return BadRequest("Page size cannot exceed 100.");
                }

                var result = await _notificationService.GetAllNotificationsAsync(
                    page, pageSize, buildingId, customerId, tenantId, type, status, warningLevel, isUrgent, searchTerm);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a notification by its ID.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>The notification if found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponseDTO>> GetNotificationById(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                
                if (notification == null)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="createDto">The notification data.</param>
        /// <returns>The created notification.</returns>
        [HttpPost]
        public async Task<ActionResult<NotificationResponseDTO>> CreateNotification([FromBody] CreateNotificationDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // For now, using a hardcoded user ID. In a real application, this would come from authentication
                int createdBy = 1; // TODO: Get from authenticated user context

                var notification = await _notificationService.CreateNotificationAsync(createDto, createdBy);
                
                return CreatedAtAction(
                    nameof(GetNotificationById), 
                    new { id = notification.NotificationId }, 
                    notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing notification.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="updateDto">The updated notification data.</param>
        /// <returns>The updated notification.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<NotificationResponseDTO>> UpdateNotification(int id, [FromBody] UpdateNotificationDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = await _notificationService.UpdateNotificationAsync(id, updateDto);
                
                if (notification == null)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="statusDto">The status update data.</param>
        /// <returns>The updated notification.</returns>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<NotificationResponseDTO>> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusDTO statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = await _notificationService.UpdateNotificationStatusAsync(id, statusDto);
                
                if (notification == null)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the notification status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Soft deletes a notification by setting IsActive to false.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>Success status.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Permanently deletes a notification from the database.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>Success status.</returns>
        [HttpDelete("{id}/permanent")]
        public async Task<ActionResult> PermanentDeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.PermanentDeleteNotificationAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while permanently deleting the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Sends a notification (updates status and timestamps).
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>Success status.</returns>
        [HttpPost("{id}/send")]
        public async Task<ActionResult> SendNotification(int id)
        {
            try
            {
                var result = await _notificationService.SendNotificationAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(new { message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>Success status.</returns>
        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(new { message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking the notification as read.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets notifications for a specific building.
        /// </summary>
        /// <param name="buildingId">The building ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<NotificationListResponseDTO>> GetNotificationsByBuilding(
            int buildingId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeRead = true)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                if (pageSize > 100)
                {
                    return BadRequest("Page size cannot exceed 100.");
                }

                var result = await _notificationService.GetNotificationsByBuildingAsync(buildingId, page, pageSize, includeRead);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving building notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets notifications for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the customer.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<NotificationListResponseDTO>> GetNotificationsByCustomer(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeRead = true)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                if (pageSize > 100)
                {
                    return BadRequest("Page size cannot exceed 100.");
                }

                var result = await _notificationService.GetNotificationsByCustomerAsync(customerId, page, pageSize, includeRead);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customer notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets notifications for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="includeRead">Whether to include read notifications.</param>
        /// <returns>A paginated list of notifications for the tenant.</returns>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<NotificationListResponseDTO>> GetNotificationsByTenant(
            int tenantId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeRead = true)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                if (pageSize > 100)
                {
                    return BadRequest("Page size cannot exceed 100.");
                }

                var result = await _notificationService.GetNotificationsByTenantAsync(tenantId, page, pageSize, includeRead);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving tenant notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets urgent/high-priority notifications.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of urgent notifications.</returns>
        [HttpGet("urgent")]
        public async Task<ActionResult<NotificationListResponseDTO>> GetUrgentNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                if (pageSize > 100)
                {
                    return BadRequest("Page size cannot exceed 100.");
                }

                var result = await _notificationService.GetUrgentNotificationsAsync(page, pageSize);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving urgent notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets notification statistics.
        /// </summary>
        /// <param name="buildingId">Optional building ID filter.</param>
        /// <param name="customerId">Optional customer ID filter.</param>
        /// <param name="tenantId">Optional tenant ID filter.</param>
        /// <returns>Notification statistics.</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetNotificationStatistics(
            [FromQuery] int? buildingId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _notificationService.GetNotificationStatisticsAsync(buildingId, customerId, tenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notification statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Schedules a notification for future sending.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="scheduledAt">When to send the notification.</param>
        /// <returns>Success status.</returns>
        [HttpPost("{id}/schedule")]
        public async Task<ActionResult> ScheduleNotification(int id, [FromBody] [Required] DateTime scheduledAt)
        {
            try
            {
                if (scheduledAt <= DateTime.UtcNow)
                {
                    return BadRequest("Scheduled time must be in the future.");
                }

                var result = await _notificationService.ScheduleNotificationAsync(id, scheduledAt);
                
                if (!result)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                return Ok(new { message = "Notification scheduled successfully.", scheduledAt });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while scheduling the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets scheduled notifications that are ready to be sent.
        /// </summary>
        /// <returns>List of notifications ready to be sent.</returns>
        [HttpGet("scheduled/ready")]
        public async Task<ActionResult<List<NotificationResponseDTO>>> GetScheduledNotificationsReadyToSend()
        {
            try
            {
                var result = await _notificationService.GetScheduledNotificationsReadyToSendAsync();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving scheduled notifications.", error = ex.Message });
            }
        }
    }
}

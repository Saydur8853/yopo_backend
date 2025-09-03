using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.NotificationCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new notification.
    /// </summary>
    public class CreateNotificationDTO
    {
        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        [Required]
        [StringLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description/content of the notification.
        /// </summary>
        [Required]
        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of notification.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Type must not exceed 50 characters.")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who the notification is sent to.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "SentTo must not exceed 100 characters.")]
        public string SentTo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who sent the notification.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "SendFrom must not exceed 100 characters.")]
        public string SendFrom { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the notification.
        /// </summary>
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Gets or sets the building ID that this notification is associated with.
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID that this notification is associated with.
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID that this notification is specifically sent to.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the warning level of the notification.
        /// </summary>
        [StringLength(20, ErrorMessage = "Warning level must not exceed 20 characters.")]
        public string? WarningLevel { get; set; }

        /// <summary>
        /// Gets or sets the priority of the notification (1-5, where 1 is highest priority).
        /// </summary>
        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Gets or sets the category of the notification.
        /// </summary>
        [StringLength(100, ErrorMessage = "Category must not exceed 100 characters.")]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets file attachments or document references (JSON format).
        /// </summary>
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets when the notification should be sent (for scheduled notifications).
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification expires.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets whether the notification requires acknowledgment from recipients.
        /// </summary>
        public bool RequiresAcknowledgment { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification is urgent.
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification should be sent via email.
        /// </summary>
        public bool SendEmail { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the notification should be sent via SMS.
        /// </summary>
        public bool SendSMS { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification should be sent via push notification.
        /// </summary>
        public bool SendPush { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the notification should be displayed in the app/system.
        /// </summary>
        public bool SendInApp { get; set; } = true;

        /// <summary>
        /// Gets or sets additional metadata or custom fields (JSON format).
        /// </summary>
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing notification.
    /// </summary>
    public class UpdateNotificationDTO
    {
        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        [Required]
        [StringLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description/content of the notification.
        /// </summary>
        [Required]
        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of notification.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Type must not exceed 50 characters.")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who the notification is sent to.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "SentTo must not exceed 100 characters.")]
        public string SentTo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who sent the notification.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "SendFrom must not exceed 100 characters.")]
        public string SendFrom { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the notification.
        /// </summary>
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Gets or sets the building ID that this notification is associated with.
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID that this notification is associated with.
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID that this notification is specifically sent to.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the warning level of the notification.
        /// </summary>
        [StringLength(20, ErrorMessage = "Warning level must not exceed 20 characters.")]
        public string? WarningLevel { get; set; }

        /// <summary>
        /// Gets or sets the priority of the notification (1-5, where 1 is highest priority).
        /// </summary>
        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Gets or sets the category of the notification.
        /// </summary>
        [StringLength(100, ErrorMessage = "Category must not exceed 100 characters.")]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets file attachments or document references (JSON format).
        /// </summary>
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets when the notification should be sent (for scheduled notifications).
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification expires.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets whether the notification requires acknowledgment from recipients.
        /// </summary>
        public bool RequiresAcknowledgment { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification is urgent.
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification should be sent via email.
        /// </summary>
        public bool SendEmail { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the notification should be sent via SMS.
        /// </summary>
        public bool SendSMS { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the notification should be sent via push notification.
        /// </summary>
        public bool SendPush { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the notification should be displayed in the app/system.
        /// </summary>
        public bool SendInApp { get; set; } = true;

        /// <summary>
        /// Gets or sets additional metadata or custom fields (JSON format).
        /// </summary>
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// DTO for notification response data.
    /// </summary>
    public class NotificationResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification.
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description/content of the notification.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of notification.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who the notification is sent to.
        /// </summary>
        public string SentTo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who sent the notification.
        /// </summary>
        public string SendFrom { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the notification.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the building ID that this notification is associated with.
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the building name if associated with a building.
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the customer ID that this notification is associated with.
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name if associated with a customer.
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID that this notification is specifically sent to.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant name if associated with a tenant.
        /// </summary>
        public string? TenantName { get; set; }

        /// <summary>
        /// Gets or sets the warning level of the notification.
        /// </summary>
        public string? WarningLevel { get; set; }

        /// <summary>
        /// Gets or sets the priority of the notification.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the category of the notification.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets file attachments or document references.
        /// </summary>
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets when the notification should be sent.
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification was actually sent.
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification was delivered to recipients.
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification was read/acknowledged.
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Gets or sets when the notification expires.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets whether the notification requires acknowledgment from recipients.
        /// </summary>
        public bool RequiresAcknowledgment { get; set; }

        /// <summary>
        /// Gets or sets whether the notification is urgent.
        /// </summary>
        public bool IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets whether the notification should be sent via email.
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// Gets or sets whether the notification should be sent via SMS.
        /// </summary>
        public bool SendSMS { get; set; }

        /// <summary>
        /// Gets or sets whether the notification should be sent via push notification.
        /// </summary>
        public bool SendPush { get; set; }

        /// <summary>
        /// Gets or sets whether the notification should be displayed in the app/system.
        /// </summary>
        public bool SendInApp { get; set; }

        /// <summary>
        /// Gets or sets additional metadata or custom fields.
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients the notification was sent to.
        /// </summary>
        public int RecipientCount { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients who have read the notification.
        /// </summary>
        public int ReadCount { get; set; }

        /// <summary>
        /// Gets or sets whether the notification record is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this notification.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created this notification.
        /// </summary>
        public string? CreatedByName { get; set; }
    }

    /// <summary>
    /// DTO for notification list response with pagination support.
    /// </summary>
    public class NotificationListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of notifications.
        /// </summary>
        public List<NotificationResponseDTO> Notifications { get; set; } = new List<NotificationResponseDTO>();

        /// <summary>
        /// Gets or sets the total count of notifications.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO for updating notification status.
    /// </summary>
    public class UpdateNotificationStatusDTO
    {
        /// <summary>
        /// Gets or sets the new status of the notification.
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the status was updated (optional, will default to current time).
        /// </summary>
        public DateTime? StatusUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets additional notes about the status update.
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes must not exceed 500 characters.")]
        public string? Notes { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.NotificationCRUD.Models
{
    /// <summary>
    /// Represents a notification entity in the system.
    /// Module: NotificationCRUD (Module ID: 14)
    /// </summary>
    [Table("Notifications")]
    public class Notification : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description/content of the notification.
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of notification (e.g., Alert, Info, Warning, Emergency, Maintenance, Event, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who the notification is sent to (e.g., All, Tenants, Owners, Specific User, Building Management, etc.).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SentTo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets who sent the notification (e.g., System, Admin, Building Manager, etc.).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SendFrom { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the notification (e.g., Draft, Sent, Delivered, Read, Failed, Scheduled, etc.).
        /// </summary>
        [Required]
        [StringLength(50)]
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
        /// Gets or sets the tenant ID that this notification is specifically sent to (optional for targeted notifications).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the warning level of the notification (e.g., Low, Medium, High, Critical, Emergency).
        /// </summary>
        [StringLength(20)]
        public string? WarningLevel { get; set; }

        /// <summary>
        /// Gets or sets the priority of the notification (1-5, where 1 is highest priority).
        /// </summary>
        [Range(1, 5)]
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Gets or sets the category of the notification (e.g., Security, Maintenance, Event, Billing, etc.).
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets file attachments or document references (JSON format for multiple files).
        /// </summary>
        [Column(TypeName = "json")]
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets when the notification should be sent (for scheduled notifications).
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
        /// Gets or sets when the notification expires (optional).
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
        [Column(TypeName = "json")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients the notification was sent to.
        /// </summary>
        public int RecipientCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of recipients who have read the notification.
        /// </summary>
        public int ReadCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether the notification record is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this notification.
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property to the building this notification is associated with.
        /// </summary>
        [ForeignKey(nameof(BuildingId))]
        public virtual YopoBackend.Modules.BuildingCRUD.Models.Building? Building { get; set; }

        /// <summary>
        /// Navigation property to the customer this notification is associated with.
        /// </summary>
        [ForeignKey(nameof(CustomerId))]
        public virtual YopoBackend.Modules.CustomerCRUD.Models.Customer? Customer { get; set; }

        /// <summary>
        /// Navigation property to the tenant this notification is specifically sent to.
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual YopoBackend.Modules.TenantCRUD.Models.Tenant? Tenant { get; set; }

        /// <summary>
        /// Navigation property to the user who created this notification.
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual YopoBackend.Modules.UserCRUD.Models.User? Creator { get; set; }
    }
}

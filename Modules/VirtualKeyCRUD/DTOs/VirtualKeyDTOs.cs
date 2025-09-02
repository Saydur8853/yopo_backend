using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.VirtualKeyCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new virtual key.
    /// Module ID: 10 (VirtualKeyCRUD)
    /// </summary>
    public class CreateVirtualKeyDto
    {
        /// <summary>
        /// Gets or sets the description of the virtual key.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the virtual key (e.g., Temporary, Permanent, Guest).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the virtual key (e.g., Active, Inactive, Expired).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets the expiration date of the virtual key.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Gets or sets the access location or description of where the key provides access.
        /// </summary>
        [StringLength(500)]
        public string? AccessLocation { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this virtual key provides access.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the intercom ID associated with this virtual key (optional).
        /// </summary>
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID if this virtual key is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the PIN code associated with the virtual key.
        /// </summary>
        [StringLength(20)]
        public string? PinCode { get; set; }

        /// <summary>
        /// Gets or sets the QR code data for the virtual key.
        /// </summary>
        [StringLength(2000)]
        public string? QrCode { get; set; }

        /// <summary>
        /// Gets or sets whether the virtual key is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of times this virtual key can be used (null for unlimited).
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (start time).
        /// </summary>
        public DateTime? ValidFromTime { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (end time).
        /// </summary>
        public DateTime? ValidToTime { get; set; }

        /// <summary>
        /// Gets or sets additional notes about the virtual key.
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing virtual key.
    /// </summary>
    public class UpdateVirtualKeyDto
    {
        /// <summary>
        /// Gets or sets the description of the virtual key.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the virtual key (e.g., Temporary, Permanent, Guest).
        /// </summary>
        [StringLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the virtual key (e.g., Active, Inactive, Expired).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets the expiration date of the virtual key.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Gets or sets the access location or description of where the key provides access.
        /// </summary>
        [StringLength(500)]
        public string? AccessLocation { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this virtual key provides access.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the intercom ID associated with this virtual key (optional).
        /// </summary>
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID if this virtual key is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the PIN code associated with the virtual key.
        /// </summary>
        [StringLength(20)]
        public string? PinCode { get; set; }

        /// <summary>
        /// Gets or sets the QR code data for the virtual key.
        /// </summary>
        [StringLength(2000)]
        public string? QrCode { get; set; }

        /// <summary>
        /// Gets or sets whether the virtual key is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of times this virtual key can be used (null for unlimited).
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (start time).
        /// </summary>
        public DateTime? ValidFromTime { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (end time).
        /// </summary>
        public DateTime? ValidToTime { get; set; }

        /// <summary>
        /// Gets or sets additional notes about the virtual key.
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for virtual key response data.
    /// </summary>
    public class VirtualKeyDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the virtual key.
        /// </summary>
        public int KeyId { get; set; }

        /// <summary>
        /// Gets or sets the description of the virtual key.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the virtual key.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the type of the virtual key (e.g., Temporary, Permanent, Guest).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the virtual key (e.g., Active, Inactive, Expired).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiration date of the virtual key.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Gets or sets the access location or description of where the key provides access.
        /// </summary>
        public string? AccessLocation { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this virtual key provides access.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the intercom ID associated with this virtual key (optional).
        /// </summary>
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID if this virtual key is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the PIN code associated with the virtual key.
        /// </summary>
        public string? PinCode { get; set; }

        /// <summary>
        /// Gets or sets the QR code data for the virtual key.
        /// </summary>
        public string? QrCode { get; set; }

        /// <summary>
        /// Gets or sets whether the virtual key is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the number of times this virtual key has been used.
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times this virtual key can be used (null for unlimited).
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Gets or sets the last time this virtual key was used.
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (start time).
        /// </summary>
        public DateTime? ValidFromTime { get; set; }

        /// <summary>
        /// Gets or sets the time range when this key is valid (end time).
        /// </summary>
        public DateTime? ValidToTime { get; set; }

        /// <summary>
        /// Gets or sets additional notes about the virtual key.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this virtual key record.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the building name (from navigation property).
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the intercom name (from navigation property).
        /// </summary>
        public string? IntercomName { get; set; }

        /// <summary>
        /// Gets or sets the tenant name (from navigation property).
        /// </summary>
        public string? TenantName { get; set; }
    }

    /// <summary>
    /// DTO for virtual key summary information (lightweight version).
    /// </summary>
    public class VirtualKeySummaryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the virtual key.
        /// </summary>
        public int KeyId { get; set; }

        /// <summary>
        /// Gets or sets the description of the virtual key.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the virtual key.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the virtual key.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiration date of the virtual key.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Gets or sets the access location.
        /// </summary>
        public string? AccessLocation { get; set; }

        /// <summary>
        /// Gets or sets the building ID.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets whether the virtual key is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the building name (from navigation property).
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the tenant name (from navigation property).
        /// </summary>
        public string? TenantName { get; set; }
    }

    /// <summary>
    /// DTO for virtual key usage tracking.
    /// </summary>
    public class VirtualKeyUsageDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the virtual key.
        /// </summary>
        public int KeyId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the key was used.
        /// </summary>
        public DateTime UsedAt { get; set; }

        /// <summary>
        /// Gets or sets the location where the key was used.
        /// </summary>
        public string? UsageLocation { get; set; }

        /// <summary>
        /// Gets or sets additional usage details.
        /// </summary>
        public string? UsageDetails { get; set; }
    }
}

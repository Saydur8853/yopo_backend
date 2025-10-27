using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace YopoBackend.Modules.InvitationCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new invitation
    /// </summary>
    public class CreateInvitationDTO
    {
        /// <summary>
        /// Gets or sets the email address of the person being invited.
        /// </summary>
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user type ID for the invitation.
        /// 
        /// Allowed Values include: Super Admin, Property Manager, Tenant, and PM-created user types.
        /// Use GET /api/invitations/user-types to get the current available user types.
        /// </summary>
        [Required]
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the company name (optional, required for Property Manager invitations).
        /// This company name will be stored in the Customer table when the Property Manager registers.
        /// </summary>
        [StringLength(300)]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the number of days until the invitation expires (1-7 days).
        /// </summary>
        [Range(1, 7)]
        public int ExpiryDays { get; set; } = 7; // Default 7 days

        /// <summary>
        /// Optional: For Property Manager inviting non-PM users, select buildings to grant access.
        /// Accepts ["all"] to grant access to all PM buildings. Ignored when inviting a Property Manager.
        /// </summary>
        public List<JsonElement>? BuildingIds { get; set; }

        /// <summary>
        /// For Tenant invitations: allocated Building ID (required for Tenant invites).
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// For Tenant invitations: allocated Floor ID (optional).
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// For Tenant invitations: allocated Unit ID (required for Tenant invites).
        /// </summary>
        public int? UnitId { get; set; }
    }

    /// <summary>
    /// DTO for updating an invitation
    /// </summary>
    public class UpdateInvitationDTO
    {
        /// <summary>
        /// Gets or sets the email address of the person being invited (optional for updates).
        /// </summary>
        [EmailAddress]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the user type ID for the invitation (optional for updates).
        /// </summary>
        public int? UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the company name (optional for updates).
        /// </summary>
        [StringLength(300)]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the number of days until the invitation expires (optional for updates, 1-7 days).
        /// </summary>
        [Range(1, 7)]
        public int? ExpiryDays { get; set; }

        /// <summary>
        /// Optional: For PM invitations to non-PM users, updates the selected building IDs.
        /// Accepts ["all"] to grant access to all PM buildings. If provided, replaces previous selection.
        /// If empty list provided, clears selection.
        /// </summary>
        public List<JsonElement>? BuildingIds { get; set; }
    }

    /// <summary>
    /// DTO for invitation response
    /// </summary>
    public class InvitationResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invitation.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the email address of the person being invited.
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user type ID for the invitation.
        /// </summary>
        public int UserTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the user type name for display purposes.
        /// </summary>
        public string UserTypeName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the company name for Property Manager invitations.
        /// </summary>
        public string? CompanyName { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the invitation expires.
        /// </summary>
        public DateTime ExpiryTime { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the invitation was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the invitation was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the invitation has expired.
        /// </summary>
        public bool IsExpired { get; set; }
        
        /// <summary>
        /// Gets or sets the number of days until the invitation expires.
        /// </summary>
        public int DaysUntilExpiry { get; set; }

        /// <summary>
        /// Gets or sets the selected building IDs (if any) associated with this invitation.
        /// Empty when none were selected.
        /// </summary>
        public List<int> BuildingIds { get; set; } = new();

        /// <summary>
        /// For Tenant invitations: allocated Building ID.
        /// </summary>
        public int? BuildingId { get; set; }
        /// <summary>
        /// For Tenant invitations: allocated Floor ID.
        /// </summary>
        public int? FloorId { get; set; }
        /// <summary>
        /// For Tenant invitations: allocated Unit ID.
        /// </summary>
        public int? UnitId { get; set; }
    }

    /// <summary>
    /// DTO for paginated invitation list response.
    /// </summary>
    public class InvitationListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of invitations.
        /// </summary>
        public List<InvitationResponseDTO> Invitations { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of invitations.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for user type dropdown selection in invitations.
    /// 
    /// **Security Note:** This DTO only contains user types that are allowed for invitations:
    /// - Super Admin (ID: 1)
    /// - Property Manager (ID: 2)
    /// 
    /// Other user types in the system are not included for security reasons.
    /// </summary>
    public class UserTypeDropdownDTO
    {
        /// <summary>
        /// Gets or sets the user type ID.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the user type name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        
        /// <summary>
        /// Gets or sets whether this user type is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}

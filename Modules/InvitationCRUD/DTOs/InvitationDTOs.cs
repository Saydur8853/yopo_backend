using System.ComponentModel.DataAnnotations;

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
        /// Gets or sets the user roll for the invitation.
        /// </summary>
        [Required]
        public string UserRoll { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of days until the invitation expires (1-7 days).
        /// </summary>
        [Range(1, 7)]
        public int ExpiryDays { get; set; } = 7; // Default 7 days
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
        /// Gets or sets the user roll for the invitation (optional for updates).
        /// </summary>
        public string? UserRoll { get; set; }

        /// <summary>
        /// Gets or sets the number of days until the invitation expires (optional for updates, 1-7 days).
        /// </summary>
        [Range(1, 7)]
        public int? ExpiryDays { get; set; }
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
        /// Gets or sets the user roll for the invitation.
        /// </summary>
        public string UserRoll { get; set; } = string.Empty;
        
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
    }
}

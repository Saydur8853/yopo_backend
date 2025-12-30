using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.IntercomAccess.DTOs
{
    public class SetMasterPinDTO
    {
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Pin { get; set; } = string.Empty;
    }

    public class SetUserPinDTO
    {
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Pin { get; set; } = string.Empty;

        // When SuperAdmin resets someone else's pin, require masterPin for authentication
        [MaxLength(20)]
        public string? MasterPin { get; set; }
    }

    public class UpdateOwnPinDTO
    {
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string NewPin { get; set; } = string.Empty;

        [MinLength(4)]
        [MaxLength(20)]
        public string? OldPin { get; set; }
    }

    public class VerifyPinDTO
    {
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Pin { get; set; } = string.Empty;
    }

    public class PinOperationResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class VerifyPinResponseDTO
    {
        public bool Granted { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CredentialType { get; set; } = string.Empty; // Master/User/Temporary/None
        public int? CredentialRefId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // New DTOs for intercom access codes (currently PIN-only)
    public class CreateAccessCodeDTO
    {
        [Required]
        public int BuildingId { get; set; }

        public int? IntercomId { get; set; } // optional; if null, applies to all intercoms in building

        // Optional tenant owner for this access code
        public int? TenantId { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(200)]
        public string Code { get; set; } = string.Empty; // raw secret to be hashed server-side

        // true => one-time use; false => reusable while active
        public bool IsSingleUse { get; set; } = false;

        // null => active immediately
        public DateTime? ValidFrom { get; set; }

        // null => infinite (no expiry)
        public DateTime? ExpiresAt { get; set; }
    }

    // DTO for updating an existing access code (only mutable fields)
    public class UpdateAccessCodeDTO
    {
        // If provided, code will be re-hashed and replaced
        [MinLength(4)]
        [MaxLength(200)]
        public string? Code { get; set; }

        // null => leave unchanged
        public bool? IsSingleUse { get; set; }

        // null => leave unchanged
        public DateTime? ValidFrom { get; set; }

        // null => infinite (no expiry); if null, leave unchanged
        public DateTime? ExpiresAt { get; set; }
    }

    public class AccessCodeDTO
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int? IntercomId { get; set; }
        public int? TenantId { get; set; }
        public string? Code { get; set; } // raw PIN if available (may be null for older records)
        public bool IsSingleUse { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

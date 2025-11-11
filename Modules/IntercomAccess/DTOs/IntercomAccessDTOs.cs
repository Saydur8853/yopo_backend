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

    public class CreateTemporaryPinDTO
    {
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string Pin { get; set; } = string.Empty;

        // Either absolute expiry or duration minutes must be provided
        public DateTime? ExpiresAt { get; set; }
        public int? ValidForMinutes { get; set; }

        // Default 1 (single-use)
        [Range(1, 100)]
        public int MaxUses { get; set; } = 1;
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

    // New DTOs for generic access codes (QR or PIN)
    public class CreateAccessCodeDTO
    {
        [Required]
        public int BuildingId { get; set; }

        public int? IntercomId { get; set; } // optional; if null, applies to all intercoms in building

        [Required]
        [RegularExpression("^(QR|PIN)$", ErrorMessage = "Type must be 'QR' or 'PIN'.")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MinLength(4)]
        [MaxLength(200)]
        public string Code { get; set; } = string.Empty; // raw secret to be hashed server-side

        // null => infinite (no expiry)
        public DateTime? ExpiresAt { get; set; }
    }

    public class AccessCodeDTO
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        public int? IntercomId { get; set; }
        public string Type { get; set; } = string.Empty; // QR or PIN
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

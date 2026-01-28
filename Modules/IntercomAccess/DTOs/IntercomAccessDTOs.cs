using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

    public class DeviceInfoDTO
    {
        [Required]
        [MaxLength(20)]
        public string Platform { get; set; } = string.Empty; // android|ios

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(50)]
        public string? AppVersion { get; set; }
    }

    public class FaceBiometricPayloadDTO
    {
        public string? FrontImageBase64 { get; set; }
        public string? LeftImageBase64 { get; set; }
        public string? RightImageBase64 { get; set; }
        public DeviceInfoDTO? DeviceInfo { get; set; }
    }

    public class VerifyAccessDTO : IValidatableObject
    {
        [MinLength(4)]
        [MaxLength(200)]
        public string? Pin { get; set; }

        public FaceBiometricPayloadDTO? Face { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Pin) && Face == null)
            {
                yield return new ValidationResult("Provide either Pin or Face payload.", new[] { nameof(Pin), nameof(Face) });
                yield break;
            }

            if (Face != null)
            {
                if (string.IsNullOrWhiteSpace(Face.FrontImageBase64) ||
                    string.IsNullOrWhiteSpace(Face.LeftImageBase64) ||
                    string.IsNullOrWhiteSpace(Face.RightImageBase64))
                {
                    yield return new ValidationResult("All three face images are required.", new[] { nameof(Face.FrontImageBase64), nameof(Face.LeftImageBase64), nameof(Face.RightImageBase64) });
                }

                if (Face.DeviceInfo == null || string.IsNullOrWhiteSpace(Face.DeviceInfo.Platform))
                {
                    yield return new ValidationResult("DeviceInfo.platform is required.", new[] { nameof(Face.DeviceInfo) });
                }
            }
        }
    }

    public class FaceBiometricUploadDTO : FaceBiometricPayloadDTO, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FrontImageBase64) ||
                string.IsNullOrWhiteSpace(LeftImageBase64) ||
                string.IsNullOrWhiteSpace(RightImageBase64))
            {
                yield return new ValidationResult("All three face images are required.", new[] { nameof(FrontImageBase64), nameof(LeftImageBase64), nameof(RightImageBase64) });
            }

            if (DeviceInfo == null || string.IsNullOrWhiteSpace(DeviceInfo.Platform))
            {
                yield return new ValidationResult("DeviceInfo.platform is required.", new[] { nameof(DeviceInfo) });
            }
        }
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
        // Required for non-tenant users; auto-resolved for tenants
        public int? BuildingId { get; set; }

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

    public class FaceBiometricRecordDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<string> Files { get; set; } = new();
        public DeviceInfoDTO? DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.VerifyIdentity.DTOs
{
    public class CreateIdentityVerificationDTO
    {
        [Required]
        [MinLength(1)]
        public List<IdentityVerificationDocumentUploadDTO> Documents { get; set; } = new();
    }

    public class IdentityVerificationDocumentUploadDTO
    {
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        public string? DocumentImageFront { get; set; }

        public string? DocumentImageBack { get; set; }

        public string? DocumentPdf { get; set; }
    }

    public class UpdateIdentityVerificationStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateIdentityVerificationDocumentsDTO
    {
        [Required]
        [MinLength(1)]
        public List<IdentityVerificationDocumentUploadDTO> Documents { get; set; } = new();
    }

    public class IdentityVerificationResponseDTO
    {
        public int RequestId { get; set; }
        public int TenantId { get; set; }
        public int? BuildingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<IdentityVerificationDocumentResponseDTO> Documents { get; set; } = new();
    }

    public class IdentityVerificationDocumentResponseDTO
    {
        public int DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

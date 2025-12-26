using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.TermsConditionsCRUD.DTOs
{
    public class TermsAndConditionResponseDTO
    {
        public int TermsAndConditionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string UsedPlace { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTermsAndConditionDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string UsedPlace { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdateTermsAndConditionDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? UsedPlace { get; set; }

        public string? Description { get; set; }
    }
}

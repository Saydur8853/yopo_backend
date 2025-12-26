using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.TermsConditionsCRUD.Models
{
    [Table("TermsAndConditions")]
    public class TermsAndCondition
    {
        [Key]
        public int TermsAndConditionId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("UsedPlace")]
        public string UsedPlace { get; set; } = string.Empty;

        [Required]
        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}

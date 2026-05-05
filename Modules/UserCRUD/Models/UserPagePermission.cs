using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Stores per-user blocked page keys for frontend page-level access control.
    /// </summary>
    [Table("UserPagePermissions")]
    public class UserPagePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("PageKey")]
        public string PageKey { get; set; } = string.Empty;

        [Column("IsBlocked")]
        public bool IsBlocked { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

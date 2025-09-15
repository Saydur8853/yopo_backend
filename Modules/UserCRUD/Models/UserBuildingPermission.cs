using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;

namespace YopoBackend.Modules.UserCRUD.Models
{
    /// <summary>
    /// Represents the relationship between users and buildings for access control.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    [Table("UserBuildingPermissions")]
    public class UserBuildingPermission
    {
        /// <summary>
        /// Gets or sets the unique identifier for the permission.
        /// </summary>
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        [Required]
        [Column("UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the building ID.
        /// </summary>
        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets whether this permission is active.
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets when this permission was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when this permission was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user associated with this permission.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Gets or sets the building associated with this permission.
        /// </summary>
        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }
    }
}
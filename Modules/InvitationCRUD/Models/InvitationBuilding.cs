using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YopoBackend.Modules.InvitationCRUD.Models
{
    /// <summary>
    /// Join table mapping an invitation to buildings that the invited user will have access to upon registration.
    /// </summary>
    [Table("InvitationBuildings")]
    public class InvitationBuilding
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("InvitationId")]
        public int InvitationId { get; set; }

        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
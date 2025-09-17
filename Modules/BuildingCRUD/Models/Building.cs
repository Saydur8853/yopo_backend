using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Services;

namespace YopoBackend.Modules.BuildingCRUD.Models
{
    /// <summary>
    /// Represents a building managed by customers (Property Managers).
    /// Module: BuildingCRUD (Module ID: 4)
    /// </summary>
    [Table("Buildings")]
    public class Building : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the building.
        /// </summary>
        [Key]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID who owns/manages this building.
        /// </summary>
        [Required]
        [Column("CustomerId")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("Address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of floors in the building.
        /// </summary>
        [Column("Floors")]
        public int Floors { get; set; }

        /// <summary>
        /// Gets or sets the number of parking floors in the building.
        /// </summary>
        [Column("ParkingFloor")]
        public int ParkingFloor { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a gym.
        /// </summary>
        [Column("HasGym")]
        public bool HasGym { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a swimming pool.
        /// </summary>
        [Column("HasSwimmingPool")]
        public bool HasSwimmingPool { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a sauna.
        /// </summary>
        [Column("HasSauna")]
        public bool HasSauna { get; set; }

        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the ID of the user who created this building record.
        /// </summary>
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the building was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the building was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the customer who owns/manages this building.
        /// </summary>
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user who created this building record.
        /// </summary>
        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;

namespace YopoBackend.Modules.FloorCRUD.Models
{
    /// <summary>
    /// Represents a floor under a specific building.
    /// Module: FloorCRUD
    /// </summary>
    [Table("Floors")]
    public class Floor
    {
        /// <summary>
        /// Gets or sets the unique identifier for the floor.
        /// </summary>
        [Key]
        public int FloorId { get; set; }

        /// <summary>
        /// Gets or sets the building ID this floor belongs to.
        /// </summary>
        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the name of the floor (e.g., "Ground Floor").
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the floor number (e.g., 0 for ground, 1 for first).
        /// </summary>
        [Column("Number")]
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the type of floor (e.g., Residential, Commercial, Parking).
        /// </summary>
        [MaxLength(100)]
        [Column("Type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the total number of units on this floor.
        /// </summary>
        [Column("TotalUnits")]
        public int TotalUnits { get; set; }

        /// <summary>
        /// Gets or sets the total area of the floor in square feet.
        /// </summary>
        [Column("AreaSqFt")]
        public decimal? AreaSqFt { get; set; }

        /// <summary>
        /// Gets or sets whether this floor is active.
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the status of the floor (e.g., Active, UnderMaintenance).
        /// </summary>
        [MaxLength(50)]
        [Column("Status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the floor was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the floor was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the building this floor belongs to.
        /// </summary>
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;
    }
}
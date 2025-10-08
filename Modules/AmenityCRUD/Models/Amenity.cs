using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.FloorCRUD.Models;

namespace YopoBackend.Modules.AmenityCRUD.Models
{
    /// <summary>
    /// Represents an amenity/facility within a building.
    /// Module: AmenityCRUD
    /// </summary>
    [Table("Amenities")]
    public class Amenity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the amenity.
        /// </summary>
        [Key]
        public int AmenityId { get; set; }

        /// <summary>
        /// Gets or sets the building ID this amenity belongs to.
        /// </summary>
        [Required]
        [Column("BuildingId")]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the name of the amenity (e.g., "Gym", "Pool").
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of amenity (e.g., "Shared", "Private").
        /// </summary>
        [MaxLength(50)]
        [Column("Type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the amenity (rules, usage info).
        /// </summary>
        [MaxLength(1000)]
        [Column("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the floor ID if the amenity is on a specific floor (optional).
        /// </summary>
        [Column("FloorId")]
        public int? FloorId { get; set; }

        /// <summary>
        /// Gets or sets whether the amenity is currently available/operational.
        /// </summary>
        [Column("IsAvailable")]
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Gets or sets the operating hours of the amenity (e.g., "8 AM - 10 PM").
        /// </summary>
        [MaxLength(100)]
        [Column("OpenHours")]
        public string? OpenHours { get; set; }

        /// <summary>
        /// Gets or sets the access control type (e.g., "QR Code", "Face Recognition").
        /// </summary>
        [MaxLength(100)]
        [Column("AccessControl")]
        public string? AccessControl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the amenity was created.
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date and time when the amenity was last updated.
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the building this amenity belongs to.
        /// </summary>
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        /// <summary>
        /// Gets or sets the floor this amenity is on (if applicable).
        /// </summary>
        [ForeignKey("FloorId")]
        public virtual Floor? Floor { get; set; }
    }
}
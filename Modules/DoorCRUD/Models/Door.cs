using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YopoBackend.Services;

namespace YopoBackend.Modules.DoorCRUD.Models
{
    /// <summary>
    /// Represents a Door entity in the building access control system.
    /// Module: DoorCRUD (Module ID: 12)
    /// </summary>
    [Table("Doors")]
    public class Door : ICreatedByEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the door.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DoorId { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this door is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the type of the door (e.g., Main Entrance, Emergency Exit, Parking, Apartment, etc.).
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the intercom ID if this door is connected to an intercom system.
        /// </summary>
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the CCTV ID if this door is monitored by a CCTV camera.
        /// </summary>
        public int? CCTVId { get; set; }

        /// <summary>
        /// Gets or sets the date when this door record was created.
        /// </summary>
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets whether this door is currently active/operational.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this door is a fire exit.
        /// </summary>
        public bool FireExit { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this door can only be opened with a PIN code.
        /// </summary>
        public bool PinOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this door can be opened by watch command (smartwatch/app command).
        /// </summary>
        public bool CanOpenByWatchCommand { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this door provides access to car park area.
        /// </summary>
        public bool IsCarPark { get; set; } = false;

        /// <summary>
        /// Gets or sets the name/label of the door.
        /// </summary>
        [StringLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the door is placed.
        /// </summary>
        [StringLength(500)]
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the floor number where this door is located.
        /// </summary>
        public int? Floor { get; set; }

        /// <summary>
        /// Gets or sets whether this door has an automatic locking mechanism.
        /// </summary>
        public bool HasAutoLock { get; set; } = false;

        /// <summary>
        /// Gets or sets the time (in seconds) after which the door automatically locks.
        /// </summary>
        public int? AutoLockDelay { get; set; }

        /// <summary>
        /// Gets or sets whether this door supports RFID/card access.
        /// </summary>
        public bool HasCardAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this door supports biometric access.
        /// </summary>
        public bool HasBiometricAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum PIN attempts allowed before lockout.
        /// </summary>
        public int? MaxPinAttempts { get; set; }

        /// <summary>
        /// Gets or sets the lockout duration (in minutes) after maximum PIN attempts are reached.
        /// </summary>
        public int? LockoutDuration { get; set; }

        /// <summary>
        /// Gets or sets the access level required to open this door (e.g., Resident, Admin, Maintenance).
        /// </summary>
        [StringLength(50)]
        public string? AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the operating hours for this door (if applicable).
        /// </summary>
        [StringLength(100)]
        public string? OperatingHours { get; set; }

        /// <summary>
        /// Gets or sets whether this door is monitored (logs access attempts).
        /// </summary>
        public bool IsMonitored { get; set; } = true;

        /// <summary>
        /// Gets or sets additional notes or description about the door.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this door record.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property to the building where this door is installed.
        /// </summary>
        [ForeignKey(nameof(BuildingId))]
        public virtual YopoBackend.Modules.BuildingCRUD.Models.Building? Building { get; set; }

        /// <summary>
        /// Navigation property to the intercom system connected to this door (if any).
        /// </summary>
        [ForeignKey(nameof(IntercomId))]
        public virtual YopoBackend.Modules.IntercomCRUD.Models.Intercom? Intercom { get; set; }

        /// <summary>
        /// Navigation property to the CCTV camera monitoring this door (if any).
        /// </summary>
        [ForeignKey(nameof(CCTVId))]
        public virtual YopoBackend.Modules.CCTVcrud.Models.CCTV? CCTV { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.DoorCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new door.
    /// Module ID: 12 (DoorCRUD)
    /// </summary>
    public class CreateDoorDto
    {
        /// <summary>
        /// Gets or sets the building ID where this door is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the type of the door (e.g., Main Entrance, Emergency Exit, Parking, Apartment, etc.).
        /// </summary>
        [Required]
        [MaxLength(100)]
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
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the door is placed.
        /// </summary>
        [MaxLength(500)]
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
        [MaxLength(50)]
        public string? AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the operating hours for this door (if applicable).
        /// </summary>
        [MaxLength(100)]
        public string? OperatingHours { get; set; }

        /// <summary>
        /// Gets or sets whether this door is monitored (logs access attempts).
        /// </summary>
        public bool IsMonitored { get; set; } = true;

        /// <summary>
        /// Gets or sets additional notes or description about the door.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing door.
    /// </summary>
    public class UpdateDoorDto
    {
        /// <summary>
        /// Gets or sets the building ID where this door is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the type of the door (e.g., Main Entrance, Emergency Exit, Parking, Apartment, etc.).
        /// </summary>
        [Required]
        [MaxLength(100)]
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
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the door is placed.
        /// </summary>
        [MaxLength(500)]
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
        [MaxLength(50)]
        public string? AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the operating hours for this door (if applicable).
        /// </summary>
        [MaxLength(100)]
        public string? OperatingHours { get; set; }

        /// <summary>
        /// Gets or sets whether this door is monitored (logs access attempts).
        /// </summary>
        public bool IsMonitored { get; set; } = true;

        /// <summary>
        /// Gets or sets additional notes or description about the door.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for door response data.
    /// </summary>
    public class DoorDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the door.
        /// </summary>
        public int DoorId { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this door is installed.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the building name where this door is installed.
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the type of the door (e.g., Main Entrance, Emergency Exit, Parking, Apartment, etc.).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the intercom ID if this door is connected to an intercom system.
        /// </summary>
        public int? IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the intercom name if this door is connected to an intercom system.
        /// </summary>
        public string? IntercomName { get; set; }

        /// <summary>
        /// Gets or sets the CCTV ID if this door is monitored by a CCTV camera.
        /// </summary>
        public int? CCTVId { get; set; }

        /// <summary>
        /// Gets or sets the CCTV name if this door is monitored by a CCTV camera.
        /// </summary>
        public string? CCTVName { get; set; }

        /// <summary>
        /// Gets or sets the date when this door record was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets whether this door is currently active/operational.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets whether this door is a fire exit.
        /// </summary>
        public bool FireExit { get; set; }

        /// <summary>
        /// Gets or sets whether this door can only be opened with a PIN code.
        /// </summary>
        public bool PinOnly { get; set; }

        /// <summary>
        /// Gets or sets whether this door can be opened by watch command (smartwatch/app command).
        /// </summary>
        public bool CanOpenByWatchCommand { get; set; }

        /// <summary>
        /// Gets or sets whether this door provides access to car park area.
        /// </summary>
        public bool IsCarPark { get; set; }

        /// <summary>
        /// Gets or sets the name/label of the door.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the door is placed.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the floor number where this door is located.
        /// </summary>
        public int? Floor { get; set; }

        /// <summary>
        /// Gets or sets whether this door has an automatic locking mechanism.
        /// </summary>
        public bool HasAutoLock { get; set; }

        /// <summary>
        /// Gets or sets the time (in seconds) after which the door automatically locks.
        /// </summary>
        public int? AutoLockDelay { get; set; }

        /// <summary>
        /// Gets or sets whether this door supports RFID/card access.
        /// </summary>
        public bool HasCardAccess { get; set; }

        /// <summary>
        /// Gets or sets whether this door supports biometric access.
        /// </summary>
        public bool HasBiometricAccess { get; set; }

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
        public string? AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the operating hours for this door (if applicable).
        /// </summary>
        public string? OperatingHours { get; set; }

        /// <summary>
        /// Gets or sets whether this door is monitored (logs access attempts).
        /// </summary>
        public bool IsMonitored { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the door.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this door record.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

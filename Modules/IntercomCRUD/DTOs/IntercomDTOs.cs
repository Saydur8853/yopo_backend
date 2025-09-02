using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.IntercomCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new Intercom system.
    /// Module ID: 9 (IntercomCRUD)
    /// </summary>
    public class CreateIntercomDto
    {
        /// <summary>
        /// Gets or sets the name of the intercom system.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the intercom system.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the intercom system (e.g., Video, Audio, Digital).
        /// </summary>
        [StringLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the price of the intercom system.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is installed.
        /// </summary>
        public bool IsInstalled { get; set; } = false;

        /// <summary>
        /// Gets or sets the size/dimensions of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the color of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the installation date of the intercom system.
        /// </summary>
        public DateTime? DateInstalled { get; set; }

        /// <summary>
        /// Gets or sets the last service/maintenance date.
        /// </summary>
        public DateTime? ServiceDate { get; set; }

        /// <summary>
        /// Gets or sets the operating system of the intercom system.
        /// </summary>
        [StringLength(100)]
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this intercom system is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the intercom is placed.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the intercom system has CCTV integration.
        /// </summary>
        public bool HasCCTV { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom system has a PIN pad for access control.
        /// </summary>
        public bool IsPinPad { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom has touch screen functionality.
        /// </summary>
        public bool HasTouchScreen { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom supports remote access.
        /// </summary>
        public bool HasRemoteAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets the IP address of the intercom system (if network-enabled).
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the MAC address of the intercom system (if network-enabled).
        /// </summary>
        [StringLength(50)]
        public string? MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the firmware version of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the intercom system.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing Intercom system.
    /// </summary>
    public class UpdateIntercomDto
    {
        /// <summary>
        /// Gets or sets the name of the intercom system.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the intercom system.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the intercom system (e.g., Video, Audio, Digital).
        /// </summary>
        [StringLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the price of the intercom system.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is installed.
        /// </summary>
        public bool IsInstalled { get; set; } = false;

        /// <summary>
        /// Gets or sets the size/dimensions of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the color of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the installation date of the intercom system.
        /// </summary>
        public DateTime? DateInstalled { get; set; }

        /// <summary>
        /// Gets or sets the last service/maintenance date.
        /// </summary>
        public DateTime? ServiceDate { get; set; }

        /// <summary>
        /// Gets or sets the operating system of the intercom system.
        /// </summary>
        [StringLength(100)]
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this intercom system is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the intercom is placed.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the intercom system has CCTV integration.
        /// </summary>
        public bool HasCCTV { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom system has a PIN pad for access control.
        /// </summary>
        public bool IsPinPad { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom has touch screen functionality.
        /// </summary>
        public bool HasTouchScreen { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the intercom supports remote access.
        /// </summary>
        public bool HasRemoteAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets the IP address of the intercom system (if network-enabled).
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the MAC address of the intercom system (if network-enabled).
        /// </summary>
        [StringLength(50)]
        public string? MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the firmware version of the intercom system.
        /// </summary>
        [StringLength(50)]
        public string? FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the intercom system.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for Intercom system response data.
    /// </summary>
    public class IntercomDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the intercom system.
        /// </summary>
        public int IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the name of the intercom system.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the intercom system.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the intercom system.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the price of the intercom system.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Gets or sets the size/dimensions of the intercom system.
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the color of the intercom system.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the installation date of the intercom system.
        /// </summary>
        public DateTime? DateInstalled { get; set; }

        /// <summary>
        /// Gets or sets the last service/maintenance date.
        /// </summary>
        public DateTime? ServiceDate { get; set; }

        /// <summary>
        /// Gets or sets the operating system of the intercom system.
        /// </summary>
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this intercom system is installed.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the building name where this intercom system is installed.
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the intercom is placed.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the intercom system has CCTV integration.
        /// </summary>
        public bool HasCCTV { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system has a PIN pad for access control.
        /// </summary>
        public bool IsPinPad { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom has touch screen functionality.
        /// </summary>
        public bool HasTouchScreen { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom supports remote access.
        /// </summary>
        public bool HasRemoteAccess { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the intercom system.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the MAC address of the intercom system.
        /// </summary>
        public string? MacAddress { get; set; }

        /// <summary>
        /// Gets or sets the firmware version of the intercom system.
        /// </summary>
        public string? FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the intercom system.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this intercom record.
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

    /// <summary>
    /// DTO for Intercom system list item (used for pagination and listing).
    /// </summary>
    public class IntercomListDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the intercom system.
        /// </summary>
        public int IntercomId { get; set; }

        /// <summary>
        /// Gets or sets the name of the intercom system.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the intercom system.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the intercom system.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this intercom system is installed.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the building name where this intercom system is installed.
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the intercom is placed.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the intercom system has CCTV integration.
        /// </summary>
        public bool HasCCTV { get; set; }

        /// <summary>
        /// Gets or sets whether the intercom system has a PIN pad for access control.
        /// </summary>
        public bool IsPinPad { get; set; }

        /// <summary>
        /// Gets or sets the installation date of the intercom system.
        /// </summary>
        public DateTime? DateInstalled { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

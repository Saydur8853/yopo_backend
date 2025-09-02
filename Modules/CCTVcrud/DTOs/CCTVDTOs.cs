using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.CCTVcrud.DTOs
{
    /// <summary>
    /// DTO for creating a new CCTV camera.
    /// Module ID: 8 (CCTVcrud)
    /// </summary>
    public class CreateCCTVDto
    {
        /// <summary>
        /// Gets or sets the name of the CCTV camera.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the CCTV camera.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size/dimensions of the CCTV camera.
        /// </summary>
        [StringLength(50)]
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this CCTV camera is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the camera is placed.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stream URL or identifier for accessing the camera feed.
        /// </summary>
        [StringLength(1000)]
        public string? Stream { get; set; }

        /// <summary>
        /// Gets or sets whether this CCTV camera feed is publicly accessible.
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Gets or sets the tenant ID if this camera is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the resolution of the CCTV camera (e.g., 1080p, 4K).
        /// </summary>
        [StringLength(50)]
        public string? Resolution { get; set; }

        /// <summary>
        /// Gets or sets whether the camera supports night vision.
        /// </summary>
        public bool HasNightVision { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the camera supports pan/tilt/zoom functionality.
        /// </summary>
        public bool HasPTZ { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the camera is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the installation date of the camera.
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// Gets or sets the last maintenance date.
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the camera.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing CCTV camera.
    /// </summary>
    public class UpdateCCTVDto
    {
        /// <summary>
        /// Gets or sets the name of the CCTV camera.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the CCTV camera.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size/dimensions of the CCTV camera.
        /// </summary>
        [StringLength(50)]
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this CCTV camera is installed.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the camera is placed.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stream URL or identifier for accessing the camera feed.
        /// </summary>
        [StringLength(1000)]
        public string? Stream { get; set; }

        /// <summary>
        /// Gets or sets whether this CCTV camera feed is publicly accessible.
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Gets or sets the tenant ID if this camera is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the resolution of the CCTV camera (e.g., 1080p, 4K).
        /// </summary>
        [StringLength(50)]
        public string? Resolution { get; set; }

        /// <summary>
        /// Gets or sets whether the camera supports night vision.
        /// </summary>
        public bool HasNightVision { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the camera supports pan/tilt/zoom functionality.
        /// </summary>
        public bool HasPTZ { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the camera is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the installation date of the camera.
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// Gets or sets the last maintenance date.
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the camera.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for CCTV camera response data.
    /// </summary>
    public class CCTVDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the CCTV camera.
        /// </summary>
        public int CctvId { get; set; }

        /// <summary>
        /// Gets or sets the name of the CCTV camera.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the CCTV camera.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size/dimensions of the CCTV camera.
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the building ID where this CCTV camera is installed.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the camera is placed.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stream URL or identifier for accessing the camera feed.
        /// </summary>
        public string? Stream { get; set; }

        /// <summary>
        /// Gets or sets whether this CCTV camera feed is publicly accessible.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID if this camera is specifically assigned to a tenant (optional).
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the resolution of the CCTV camera (e.g., 1080p, 4K).
        /// </summary>
        public string? Resolution { get; set; }

        /// <summary>
        /// Gets or sets whether the camera supports night vision.
        /// </summary>
        public bool HasNightVision { get; set; }

        /// <summary>
        /// Gets or sets whether the camera supports pan/tilt/zoom functionality.
        /// </summary>
        public bool HasPTZ { get; set; }

        /// <summary>
        /// Gets or sets whether the camera is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the installation date of the camera.
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// Gets or sets the last maintenance date.
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Gets or sets additional notes or description about the camera.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this CCTV record.
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

        /// <summary>
        /// Gets or sets the building name (from navigation property).
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the tenant name (from navigation property).
        /// </summary>
        public string? TenantName { get; set; }
    }

    /// <summary>
    /// DTO for CCTV summary information (lightweight version).
    /// </summary>
    public class CCTVSummaryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the CCTV camera.
        /// </summary>
        public int CctvId { get; set; }

        /// <summary>
        /// Gets or sets the name of the CCTV camera.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model of the CCTV camera.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the building ID where this CCTV camera is installed.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the specific location/description of where the camera is placed.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this CCTV camera feed is publicly accessible.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets whether the camera is currently active/operational.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the building name (from navigation property).
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Gets or sets the tenant name (from navigation property).
        /// </summary>
        public string? TenantName { get; set; }
    }
}

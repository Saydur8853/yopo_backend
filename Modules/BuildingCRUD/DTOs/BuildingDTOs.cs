using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.BuildingCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new building.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class CreateBuildingDto
    {
        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the photo URL or path for the building.
        /// </summary>
        [MaxLength(1000)]
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets the type of the building (e.g., Residential, Commercial, Mixed-use).
        /// </summary>
        [MaxLength(100)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the number of floors in the building.
        /// </summary>
        public int? Floors { get; set; }

        /// <summary>
        /// Gets or sets the parking floor level (e.g., -1 for basement, 0 for ground).
        /// </summary>
        public int? ParkingFloor { get; set; }

        /// <summary>
        /// Gets or sets the number of parking spaces available.
        /// </summary>
        public int? ParkingSpace { get; set; }

        /// <summary>
        /// Gets or sets the number of residential units in the building.
        /// </summary>
        public int? Units { get; set; }

        /// <summary>
        /// Gets or sets the number of commercial units in the building.
        /// </summary>
        public int? CommercialUnit { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a gym facility.
        /// </summary>
        public bool HasGym { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a swimming pool.
        /// </summary>
        public bool HasSwimpool { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a sauna facility.
        /// </summary>
        public bool HasSauna { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a reception area.
        /// </summary>
        public bool HasReception { get; set; } = false;

        /// <summary>
        /// Gets or sets the developer/construction company name.
        /// </summary>
        [MaxLength(200)]
        public string? Developer { get; set; }

        /// <summary>
        /// Gets or sets the primary color theme of the building.
        /// </summary>
        [MaxLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the date when the building started operations.
        /// </summary>
        public DateTime? DateStartOperation { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing building.
    /// </summary>
    public class UpdateBuildingDto
    {
        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the photo URL or path for the building.
        /// </summary>
        [MaxLength(1000)]
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets the type of the building (e.g., Residential, Commercial, Mixed-use).
        /// </summary>
        [MaxLength(100)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the number of floors in the building.
        /// </summary>
        public int? Floors { get; set; }

        /// <summary>
        /// Gets or sets the parking floor level (e.g., -1 for basement, 0 for ground).
        /// </summary>
        public int? ParkingFloor { get; set; }

        /// <summary>
        /// Gets or sets the number of parking spaces available.
        /// </summary>
        public int? ParkingSpace { get; set; }

        /// <summary>
        /// Gets or sets the number of residential units in the building.
        /// </summary>
        public int? Units { get; set; }

        /// <summary>
        /// Gets or sets the number of commercial units in the building.
        /// </summary>
        public int? CommercialUnit { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a gym facility.
        /// </summary>
        public bool HasGym { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a swimming pool.
        /// </summary>
        public bool HasSwimpool { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a sauna facility.
        /// </summary>
        public bool HasSauna { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the building has a reception area.
        /// </summary>
        public bool HasReception { get; set; } = false;

        /// <summary>
        /// Gets or sets the developer/construction company name.
        /// </summary>
        [MaxLength(200)]
        public string? Developer { get; set; }

        /// <summary>
        /// Gets or sets the primary color theme of the building.
        /// </summary>
        [MaxLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the date when the building started operations.
        /// </summary>
        public DateTime? DateStartOperation { get; set; }

        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for building response data.
    /// </summary>
    public class BuildingDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the building.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the photo URL or path for the building.
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets the type of the building (e.g., Residential, Commercial, Mixed-use).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the number of floors in the building.
        /// </summary>
        public int? Floors { get; set; }

        /// <summary>
        /// Gets or sets the parking floor level (e.g., -1 for basement, 0 for ground).
        /// </summary>
        public int? ParkingFloor { get; set; }

        /// <summary>
        /// Gets or sets the number of parking spaces available.
        /// </summary>
        public int? ParkingSpace { get; set; }

        /// <summary>
        /// Gets or sets the number of residential units in the building.
        /// </summary>
        public int? Units { get; set; }

        /// <summary>
        /// Gets or sets the number of commercial units in the building.
        /// </summary>
        public int? CommercialUnit { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a gym facility.
        /// </summary>
        public bool HasGym { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a swimming pool.
        /// </summary>
        public bool HasSwimpool { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a sauna facility.
        /// </summary>
        public bool HasSauna { get; set; }

        /// <summary>
        /// Gets or sets whether the building has a reception area.
        /// </summary>
        public bool HasReception { get; set; }

        /// <summary>
        /// Gets or sets the developer/construction company name.
        /// </summary>
        public string? Developer { get; set; }

        /// <summary>
        /// Gets or sets the primary color theme of the building.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the date when the building started operations.
        /// </summary>
        public DateTime? DateStartOperation { get; set; }

        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created this building.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets when the building was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the building was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

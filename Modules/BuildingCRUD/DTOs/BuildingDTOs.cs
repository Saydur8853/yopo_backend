using System.ComponentModel.DataAnnotations;
using YopoBackend.Modules.FloorCRUD.DTOs;
using YopoBackend.Modules.UnitCRUD.DTOs;
using YopoBackend.Modules.AmenityCRUD.DTOs;

namespace YopoBackend.Modules.BuildingCRUD.DTOs
{
    /// <summary>
    /// DTO for creating a new building.
    /// Module ID: 4 (BuildingCRUD)
    /// </summary>
    public class CreateBuildingDTO
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

    }

    /// <summary>
    /// DTO for updating an existing building with partial update support.
    /// Only provide the fields you want to update - all fields are optional.
    /// </summary>
    public class UpdateBuildingDTO
    {
        /// <summary>
        /// Gets or sets the name of the building. Optional - only updated if provided.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Building name cannot exceed 200 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the address of the building. Optional - only updated if provided.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }


        /// <summary>
        /// Gets or sets whether this building is active. Optional - only updated if provided.
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for building response data.
    /// </summary>
    public class BuildingResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the building.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID who owns/manages this building.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name who owns/manages this building.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name of the customer.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the building.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the building.
        /// </summary>
        public string Address { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets whether this building is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created this building.
        /// </summary>
        public string CreatedByName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the building was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the building was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Floors that belong to this building.
        /// </summary>
        public List<FloorResponseDTO> Floors { get; set; } = new();

        /// <summary>
        /// Units that belong to this building.
        /// </summary>
        public List<UnitResponseDTO> Units { get; set; } = new();

        /// <summary>
        /// Amenities available in this building.
        /// </summary>
        public List<AmenityResponseDTO> Amenities { get; set; } = new();
    }

    /// <summary>
    /// DTO for paginated building list responses.
    /// </summary>
    public class BuildingListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of buildings.
        /// </summary>
        public List<BuildingResponseDTO> Buildings { get; set; } = new List<BuildingResponseDTO>();

        /// <summary>
        /// Gets or sets the total count of buildings.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }
    }

}

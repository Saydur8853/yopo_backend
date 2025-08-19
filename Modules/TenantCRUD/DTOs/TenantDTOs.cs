using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.TenantCRUD.DTOs
{
    /// <summary>
    /// DTO for retrieving tenant information.
    /// </summary>
    public class TenantDto
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Floor { get; set; }
        public string UnitNo { get; set; } = string.Empty;
        public int ParkingSpace { get; set; }
        public string? Contact { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool Paid { get; set; }
        public string MemberType { get; set; } = string.Empty;
        public string? Files { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new tenant.
    /// </summary>
    public class CreateTenantDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "BuildingId must be a positive integer")]
        public int BuildingId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Floor must be a non-negative integer")]
        public int Floor { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string UnitNo { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "ParkingSpace must be a non-negative integer")]
        public int ParkingSpace { get; set; }

        [StringLength(500)]
        public string? Contact { get; set; }

        [Required]
        public DateTime ContractStartDate { get; set; }

        [Required]
        public DateTime ContractEndDate { get; set; }

        public bool Paid { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string MemberType { get; set; } = string.Empty;

        public string? Files { get; set; }

        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing tenant.
    /// </summary>
    public class UpdateTenantDto
    {
        [StringLength(200, MinimumLength = 1)]
        public string? Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BuildingId must be a positive integer")]
        public int? BuildingId { get; set; }

        [StringLength(50, MinimumLength = 1)]
        public string? Type { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Floor must be a non-negative integer")]
        public int? Floor { get; set; }

        [StringLength(20, MinimumLength = 1)]
        public string? UnitNo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ParkingSpace must be a non-negative integer")]
        public int? ParkingSpace { get; set; }

        [StringLength(500)]
        public string? Contact { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool? Paid { get; set; }

        [StringLength(50, MinimumLength = 1)]
        public string? MemberType { get; set; }

        public string? Files { get; set; }

        public bool? Active { get; set; }
    }

    /// <summary>
    /// DTO for listing tenants with pagination.
    /// </summary>
    public class TenantListDto
    {
        public List<TenantDto> Tenants { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// DTO for tenant search and filtering.
    /// </summary>
    public class TenantSearchDto
    {
        public string? Name { get; set; }
        public int? BuildingId { get; set; }
        public string? Type { get; set; }
        public int? Floor { get; set; }
        public string? UnitNo { get; set; }
        public string? MemberType { get; set; }
        public bool? Paid { get; set; }
        public bool? Active { get; set; }
        public DateTime? ContractStartDateFrom { get; set; }
        public DateTime? ContractStartDateTo { get; set; }
        public DateTime? ContractEndDateFrom { get; set; }
        public DateTime? ContractEndDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

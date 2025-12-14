using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.TenantCRUD.DTOs
{
    public class CreateTenantDTO
    {
        [Required]
        [MaxLength(200)]
        public string TenantName { get; set; } = string.Empty;
        [Required]
        public int BuildingId { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool IsPaid { get; set; }
        [MaxLength(100)]
        public string? MemberType { get; set; }
        [MaxLength(1000)]
        public string? DocumentFile { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTenantDTO
    {
        [MaxLength(200)]
        public string? TenantName { get; set; }
        public int? BuildingId { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool? IsPaid { get; set; }
        [MaxLength(100)]
        public string? MemberType { get; set; }
        [MaxLength(1000)]
        public string? DocumentFile { get; set; }
        public bool? IsActive { get; set; }
    }

    public class TenantResponseDTO
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public string? Type { get; set; }
        public int? FloorId { get; set; }
        public string? FloorName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitNumber { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool IsPaid { get; set; }
        public string? MemberType { get; set; }
        public string? DocumentFile { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class TenantListResponseDTO
    {
        public List<TenantResponseDTO> Tenants { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }


}

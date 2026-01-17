using YopoBackend.Modules.VerifyIdentity.DTOs;

namespace YopoBackend.Modules.VerifyIdentity.Services
{
    public interface IVerifyIdentityService
    {
        Task<IdentityVerificationResponseDTO> CreateRequestAsync(int userId, string userRole, CreateIdentityVerificationDTO dto);
        Task<(List<IdentityVerificationResponseDTO> Requests, int TotalRecords)> GetRequestsAsync(
            int userId,
            string userRole,
            int? buildingId,
            int? tenantId,
            string? status,
            int page,
            int pageSize);
        Task<IdentityVerificationResponseDTO> UpdateDocumentsAsync(int requestId, int userId, string userRole, UpdateIdentityVerificationDocumentsDTO dto);
        Task DeleteRequestAsync(int requestId, int userId, string userRole);
        Task<IdentityVerificationResponseDTO> UpdateStatusAsync(int requestId, int userId, string userRole, string status);
    }
}

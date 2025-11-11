using YopoBackend.Modules.IntercomAccess.DTOs;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public interface IIntercomAccessService
    {
        Task<PinOperationResponseDTO> SetOrUpdateMasterPinAsync(int intercomId, string pin, int currentUserId);
        Task<PinOperationResponseDTO> SetOrUpdateUserPinAsync(int intercomId, int userId, string pin, int currentUserId, string? masterPin = null);
        Task<PinOperationResponseDTO> UpdateOwnUserPinAsync(int intercomId, int currentUserId, string newPin, string? oldPin);
        Task<PinOperationResponseDTO> CreateTemporaryPinAsync(int intercomId, int currentUserId, string pin, DateTime expiresAt, int maxUses);
        Task<VerifyPinResponseDTO> VerifyPinAsync(int intercomId, string pin, string? ip, string? deviceInfo);

        Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO> items, int total)> GetAccessLogsAsync(
            int intercomId, int page, int pageSize, DateTime? from, DateTime? to, bool? success, string? credentialType, int? userId);

        Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.TemporaryPinUsageDTO> items, int total)> GetTemporaryUsagesAsync(
            int intercomId, int page, int pageSize, DateTime? from, DateTime? to, int? temporaryPinId);

        // Access codes (QR or PIN), building-level or intercom-level
        Task<List<YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO>> GetAccessCodesAsync(int? buildingId, int? intercomId);
        Task<(bool Success, string Message, YopoBackend.Modules.IntercomAccess.DTOs.AccessCodeDTO? Code)> CreateAccessCodeAsync(YopoBackend.Modules.IntercomAccess.DTOs.CreateAccessCodeDTO dto, int currentUserId);
        Task<(bool Success, string Message)> DeactivateAccessCodeAsync(int id, int currentUserId);

        Task<(List<YopoBackend.Modules.IntercomAccess.DTOs.AccessLogDTO> items, int total)> GetAccessLogsGlobalAsync(
            int? buildingId, int? intercomId, int? codeId, int page, int pageSize, DateTime? from, DateTime? to, bool? success, string? credentialType, int? userId);
    }
}

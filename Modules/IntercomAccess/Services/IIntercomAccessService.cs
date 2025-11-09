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
    }
}

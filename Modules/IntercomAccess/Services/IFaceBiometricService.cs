using YopoBackend.Modules.IntercomAccess.DTOs;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public interface IFaceBiometricService
    {
        Task<FaceBiometricRecordDTO?> GetAsync(int userId);
        Task<(bool Success, string Message, FaceBiometricRecordDTO? Record)> UpsertAsync(int userId, FaceBiometricUploadDTO dto);
        Task<(bool Success, string Message)> DeleteAsync(int userId);
    }
}

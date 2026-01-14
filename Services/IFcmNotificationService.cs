using YopoBackend.Modules.AnnouncementCRUD.DTOs;

namespace YopoBackend.Services
{
    public interface IFcmNotificationService
    {
        Task<bool> SendAnnouncementAsync(AnnouncementResponseDTO announcement);
    }
}

using YopoBackend.Modules.AnnouncementCRUD.DTOs;

namespace YopoBackend.Modules.AnnouncementCRUD.Services
{
    public interface IAnnouncementService
    {
        Task<AnnouncementListResponseDTO> GetAnnouncementsAsync(int currentUserId, int page = 1, int pageSize = 10, int? buildingId = null);
        Task<AnnouncementResponseDTO> CreateAnnouncementAsync(CreateAnnouncementDTO dto, int currentUserId);
        Task<AnnouncementResponseDTO?> UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementDTO dto, int currentUserId);
        Task<bool> DeleteAnnouncementAsync(int announcementId, int currentUserId);
    }
}

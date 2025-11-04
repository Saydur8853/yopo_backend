using YopoBackend.Modules.IntercomCRUD.DTOs;

namespace YopoBackend.Modules.IntercomCRUD.Services
{
    public interface IIntercomService
    {
        Task<(List<IntercomResponseDTO> intercoms, int totalRecords)> GetIntercomsAsync(
            int page,
            int pageSize,
            string? searchTerm,
            int? customerId,
            int? buildingId,
            bool? isActive,
            bool? isInstalled,
            string? intercomType,
            string? operatingSystem,
            bool? hasCCTV,
            bool? hasPinPad,
            DateTime? installedFrom,
            DateTime? installedTo,
            DateTime? serviceFrom,
            DateTime? serviceTo,
            decimal? priceMin,
            decimal? priceMax,
            string? color,
            string? model
        );

        Task<(bool Success, string Message, IntercomResponseDTO? Data)> CreateIntercomAsync(CreateIntercomDTO dto);
        Task<(bool Success, string Message, IntercomResponseDTO? Data)> UpdateIntercomAsync(int intercomId, UpdateIntercomDTO dto);
        Task<(bool Success, string Message)> DeleteIntercomAsync(int intercomId);
    }
}
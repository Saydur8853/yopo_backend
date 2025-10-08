using YopoBackend.Modules.UnitCRUD.DTOs;

namespace YopoBackend.Modules.UnitCRUD.Services
{
    public interface IUnitService
    {
        Task<(bool Success, string Message, List<UnitResponseDTO>? Data)> GetUnitsByFloorAsync(int floorId);
        Task<(bool Success, string Message, UnitResponseDTO? Data)> CreateUnitAsync(CreateUnitDTO dto);
        Task<(bool Success, string Message, UnitResponseDTO? Data)> UpdateUnitAsync(int unitId, UpdateUnitDTO dto);
        Task<(bool Success, string Message)> DeleteUnitAsync(int unitId);
    }
}
using YopoBackend.Modules.UnitCRUD.DTOs;

namespace YopoBackend.Modules.UnitCRUD.Services
{
    public interface IUnitService
    {
        Task<(List<UnitResponseDTO> units, int totalRecords)> GetUnitsAsync(int? floorId, int? buildingId, int pageNumber, int pageSize);
        Task<(bool Success, string Message, UnitResponseDTO? Data)> CreateUnitAsync(CreateUnitDTO dto);
        Task<(bool Success, string Message, UnitResponseDTO? Data)> UpdateUnitAsync(int unitId, UpdateUnitDTO dto);
        Task<(bool Success, string Message)> DeleteUnitAsync(int unitId);
    }
}
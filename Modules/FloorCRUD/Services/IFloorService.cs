using YopoBackend.Modules.FloorCRUD.DTOs;

namespace YopoBackend.Modules.FloorCRUD.Services
{
    /// <summary>
    /// Interface for Floor service operations.
    /// </summary>
    public interface IFloorService
    {
        Task<(List<FloorResponseDTO> floors, int totalRecords)> GetFloorsAsync(int? buildingId, int? userId, int page, int pageSize);
        Task<FloorResponseDTO?> CreateFloorAsync(CreateFloorDTO dto);
        Task<FloorResponseDTO?> UpdateFloorAsync(int floorId, UpdateFloorDTO dto);
        Task<(bool success, string? floorName, string? buildingName)> DeleteFloorAsync(int floorId);
    }
}
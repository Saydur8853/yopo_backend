using YopoBackend.Modules.FloorCRUD.DTOs;

namespace YopoBackend.Modules.FloorCRUD.Services
{
    /// <summary>
    /// Interface for Floor service operations.
    /// </summary>
    public interface IFloorService
    {
        Task<List<FloorResponseDTO>> GetFloorsByBuildingAsync(int buildingId);
        Task<FloorResponseDTO?> CreateFloorAsync(CreateFloorDTO dto);
        Task<FloorResponseDTO?> UpdateFloorAsync(int floorId, UpdateFloorDTO dto);
        Task<bool> DeleteFloorAsync(int floorId);
    }
}
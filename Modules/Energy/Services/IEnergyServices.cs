using YopoBackend.Modules.Energy.DTOs;

namespace YopoBackend.Modules.Energy.Services
{
    public interface IQuestDbService
    {
        Task<double> GetCurrentPowerKwAsync(int buildingId, string? topicPrefix = null);
        Task<double> GetCurrentMonthConsumptionKwhAsync(int buildingId, DateTime utcNow, string? topicPrefix = null);
        Task<Dictionary<int, double>> GetHourlyAveragePowerKwAsync(int buildingId, DateTime dateUtc, string? topicPrefix = null);
        Task<List<EnergyTopicReadingDto>> GetLatestTopicReadingsAsync(int buildingId, string? topicPrefix = null, int limit = 20);
    }

    public interface IEnergyService
    {
        Task<List<EnergyLocationDto>> GetLocationsAsync(int currentUserId);
        Task<BuildingOverviewDto?> GetOverviewAsync(string locationId, int currentUserId);
        Task<List<DewaMeterDto>> GetDewaMetersAsync(string locationId, int currentUserId);
        Task<EnergyConsumptionDto?> GetEnergyConsumptionAsync(string locationId, int currentUserId);
        Task<double> GetCurrentPowerAsync(string locationId, int currentUserId);
        Task<List<HourlyDataDto>> GetHourlyAsync(string locationId, DateTime? date, int currentUserId);
        Task<List<MonthlyDataDto>> GetMonthlyAsync(string locationId, int currentUserId);
        Task<EnergyTopicLiveResponseDto?> GetLiveTopicsAsync(string locationId, int currentUserId, int limit = 20);
    }
}

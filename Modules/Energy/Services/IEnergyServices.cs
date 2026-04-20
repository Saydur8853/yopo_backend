using YopoBackend.Modules.Energy.DTOs;

namespace YopoBackend.Modules.Energy.Services
{
    public interface IInfluxDbService
    {
        Task<double> GetCurrentPowerKwAsync(string bucket, string topicPrefix);
        Task<double> GetCurrentMonthConsumptionKwhAsync(string bucket, string topicPrefix, DateTime utcNow);
        Task<Dictionary<int, double>> GetHourlyAveragePowerKwAsync(string bucket, string topicPrefix, DateTime dateUtc);
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
    }
}

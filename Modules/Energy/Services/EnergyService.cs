using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.Energy.DTOs;
using YopoBackend.Modules.Energy.Models;
using YopoBackend.Modules.UserCRUD.Models;

namespace YopoBackend.Modules.Energy.Services
{
    public class EnergyService : IEnergyService
    {
        private const decimal EffectiveRatePerKwh = 0.44m;

        private readonly ApplicationDbContext _context;
        private readonly IInfluxDbService _influxDbService;
        private readonly InfluxDbSettings _influxSettings;

        public EnergyService(
            ApplicationDbContext context,
            IInfluxDbService influxDbService,
            IOptions<InfluxDbSettings> influxOptions)
        {
            _context = context;
            _influxDbService = influxDbService;
            _influxSettings = influxOptions.Value;
        }

        public async Task<List<EnergyLocationDto>> GetLocationsAsync(int currentUserId)
        {
            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUserId);
            if (!accessibleBuildingIds.Any())
            {
                return new List<EnergyLocationDto>();
            }

            var buildings = await _context.Buildings
                .AsNoTracking()
                .Where(b => b.IsActive && accessibleBuildingIds.Contains(b.BuildingId))
                .OrderBy(b => b.Name)
                .ToListAsync();

            if (!buildings.Any())
            {
                return new List<EnergyLocationDto>();
            }

            var buildingIds = buildings.Select(b => b.BuildingId).ToList();

            var unitsByBuilding = await _context.Units
                .AsNoTracking()
                .Where(u => buildingIds.Contains(u.BuildingId))
                .GroupBy(u => u.BuildingId)
                .Select(g => new { BuildingId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BuildingId, x => x.Count);

            var floorsByBuilding = await _context.Floors
                .AsNoTracking()
                .Where(f => buildingIds.Contains(f.BuildingId))
                .GroupBy(f => f.BuildingId)
                .Select(g => new { BuildingId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BuildingId, x => x.Count);

            var energyMap = await _context.EnergyLocations
                .AsNoTracking()
                .Where(e => e.BuildingId.HasValue && buildingIds.Contains(e.BuildingId.Value))
                .GroupBy(e => e.BuildingId!.Value)
                .Select(g => g.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).First())
                .ToDictionaryAsync(e => e.BuildingId!.Value, e => e);

            var response = new List<EnergyLocationDto>(buildings.Count);
            foreach (var building in buildings)
            {
                energyMap.TryGetValue(building.BuildingId, out var energy);

                response.Add(new EnergyLocationDto
                {
                    Id = building.BuildingId.ToString(),
                    Name = building.Name,
                    ShortName = building.Name,
                    Address = building.Address,
                    City = string.Empty,
                    TotalUnits = unitsByBuilding.TryGetValue(building.BuildingId, out var units) ? units : 0,
                    Towers = new List<string>(),
                    Floors = floorsByBuilding.TryGetValue(building.BuildingId, out var floors) ? floors : 0,
                    Basements = 0,
                    BmsType = energy?.BmsType,
                    GatewayId = energy?.GatewayId,
                    OccupancyPercent = energy?.OccupancyPercent ?? 0,
                    Status = energy?.Status ?? "active",
                    ConnectedSince = energy?.ConnectedSince,
                    LastDataReceived = energy?.LastDataReceived
                });
            }

            return response;
        }

        public async Task<BuildingOverviewDto?> GetOverviewAsync(string locationId, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return null;
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var locationKey = ResolveLocationKey(building.BuildingId, energy);

            var unitsCount = await _context.Units
                .AsNoTracking()
                .CountAsync(u => u.BuildingId == building.BuildingId);

            var dewaMeterCount = await _context.EnergyDewaMeters
                .AsNoTracking()
                .CountAsync(m => m.LocationId == locationKey);

            var activeAlerts = await _context.EnergyAlerts
                .AsNoTracking()
                .CountAsync(a => a.LocationId == locationKey && a.IsActive && !a.Acknowledged);

            var criticalAlerts = await _context.EnergyAlerts
                .AsNoTracking()
                .CountAsync(a => a.LocationId == locationKey && a.IsActive && !a.Acknowledged && a.Severity == "critical");

            var status = ResolveStatus(building, energy);

            return new BuildingOverviewDto
            {
                LocationId = building.BuildingId.ToString(),
                Name = building.Name,
                Address = building.Address,
                LastUpdated = energy?.LastDataReceived,
                Status = status,
                OccupancyPercent = energy?.OccupancyPercent ?? 0,
                TotalUnits = unitsCount,
                DewaMeterCount = dewaMeterCount,
                ActiveAlerts = activeAlerts,
                CriticalAlerts = criticalAlerts
            };
        }

        public async Task<List<DewaMeterDto>> GetDewaMetersAsync(string locationId, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return new List<DewaMeterDto>();
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var locationKey = ResolveLocationKey(building.BuildingId, energy);

            var meters = await _context.EnergyDewaMeters
                .AsNoTracking()
                .Where(m => m.LocationId == locationKey)
                .Include(m => m.Bills)
                .ToListAsync();

            return meters.Select(m =>
            {
                var latestBill = m.Bills.OrderByDescending(b => b.BillMonth).FirstOrDefault();
                return new DewaMeterDto
                {
                    AccountNumber = m.AccountNumber,
                    PremiseLabel = m.PremiseLabel,
                    MeterNumber = m.MeterNumber,
                    Mf = m.MultiplicationFactor,
                    CtRatio = m.CtRatio,
                    Description = m.Description,
                    LastReading = latestBill?.MeterReadingCurrent,
                    LastKwh = latestBill?.Kwh,
                    LastAed = latestBill?.ElectricityAed,
                    HasWater = m.HasWater,
                    LastWaterAed = latestBill?.WaterAed
                };
            }).OrderBy(x => x.PremiseLabel).ToList();
        }

        public async Task<EnergyConsumptionDto?> GetEnergyConsumptionAsync(string locationId, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return null;
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var locationKey = ResolveLocationKey(building.BuildingId, energy);
            var now = DateTime.UtcNow;

            var baselineMonth = $"{now.Year - 1}-{now.Month:00}";
            var baselineYearPrefix = $"{now.Year - 1}-";

            var baselineKwh = await _context.EnergyDewaBills
                .AsNoTracking()
                .Where(b => b.LocationId == locationKey && b.BillMonth == baselineMonth)
                .SumAsync(b => (int?)b.Kwh) ?? 0;

            var baselineCost = await _context.EnergyDewaBills
                .AsNoTracking()
                .Where(b => b.LocationId == locationKey && b.BillMonth == baselineMonth)
                .SumAsync(b => (decimal?)b.ElectricityAed) ?? 0;

            var annualBaseline = await _context.EnergyDewaBills
                .AsNoTracking()
                .Where(b => b.LocationId == locationKey && EF.Functions.Like(b.BillMonth, baselineYearPrefix + "%"))
                .SumAsync(b => (decimal?)b.ElectricityAed) ?? 0;

            var bucket = ResolveBucket(energy);
            var topicPrefix = ResolveTopicPrefix(energy, building);
            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(topicPrefix))
            {
                return BuildConsumptionFallback(building.BuildingId.ToString(), baselineKwh, baselineCost, annualBaseline);
            }

            var currentKwh = await _influxDbService.GetCurrentMonthConsumptionKwhAsync(bucket, topicPrefix, now);
            var current = (int)Math.Round(currentKwh, MidpointRounding.AwayFromZero);
            return BuildConsumptionFallback(building.BuildingId.ToString(), baselineKwh, baselineCost, annualBaseline, current);
        }

        public async Task<double> GetCurrentPowerAsync(string locationId, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return 0;
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var bucket = ResolveBucket(energy);
            var topicPrefix = ResolveTopicPrefix(energy, building);
            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(topicPrefix))
            {
                return 0;
            }

            var kw = await _influxDbService.GetCurrentPowerKwAsync(bucket, topicPrefix);
            return Math.Round(kw, 2);
        }

        public async Task<List<HourlyDataDto>> GetHourlyAsync(string locationId, DateTime? date, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return new List<HourlyDataDto>();
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var locationKey = ResolveLocationKey(building.BuildingId, energy);
            var targetDate = (date ?? DateTime.UtcNow).Date;

            var baselineMonth = $"{targetDate.Year - 1}-{targetDate.Month:00}";
            var monthBaseline = await _context.EnergyDewaBills
                .AsNoTracking()
                .Where(b => b.LocationId == locationKey && b.BillMonth == baselineMonth)
                .SumAsync(b => (int?)b.Kwh) ?? 0;

            var bucket = ResolveBucket(energy);
            var topicPrefix = ResolveTopicPrefix(energy, building);
            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(topicPrefix))
            {
                return BuildEmptyHourly(monthBaseline);
            }

            var hours = await _influxDbService.GetHourlyAveragePowerKwAsync(bucket, topicPrefix, targetDate);
            var baselinePerHour = monthBaseline > 0 ? Math.Round(monthBaseline / 30d / 24d, 2) : 0;

            var result = new List<HourlyDataDto>(24);
            for (var hour = 0; hour < 24; hour++)
            {
                var power = hours.TryGetValue(hour, out var value) ? Math.Round(value, 2) : 0;
                result.Add(new HourlyDataDto
                {
                    Hour = $"{hour:00}:00",
                    Consumption = power,
                    Baseline = baselinePerHour,
                    Power = power,
                    OutdoorTemp = 0
                });
            }

            return result;
        }

        public async Task<List<MonthlyDataDto>> GetMonthlyAsync(string locationId, int currentUserId)
        {
            var building = await GetAccessibleBuildingAsync(locationId, currentUserId);
            if (building == null)
            {
                return new List<MonthlyDataDto>();
            }

            var energy = await GetEnergyLocationByBuildingIdAsync(building.BuildingId);
            var locationKey = ResolveLocationKey(building.BuildingId, energy);
            var latestYear = DateTime.UtcNow.Year - 1;
            var yearPrefix = $"{latestYear}-";

            var bills = await _context.EnergyDewaBills
                .AsNoTracking()
                .Where(b => b.LocationId == locationKey && EF.Functions.Like(b.BillMonth, yearPrefix + "%"))
                .GroupBy(b => b.BillMonth)
                .Select(g => new
                {
                    BillMonth = g.Key,
                    Kwh = g.Sum(x => x.Kwh),
                    Cost = g.Sum(x => x.ElectricityAed)
                })
                .OrderBy(g => g.BillMonth)
                .ToListAsync();

            return bills.Select(x =>
            {
                var monthIndex = int.TryParse(x.BillMonth.Split('-').LastOrDefault(), out var m) ? m : 1;
                var monthName = new DateTime(2000, Math.Clamp(monthIndex, 1, 12), 1).ToString("MMM");
                return new MonthlyDataDto
                {
                    Month = monthName,
                    Kwh = x.Kwh,
                    Cost = x.Cost,
                    BaselineKwh = x.Kwh,
                    BaselineCost = x.Cost,
                    SavingsKwh = 0,
                    SavingsPct = 0
                };
            }).ToList();
        }

        private async Task<Building?> GetBuildingAsync(string locationId)
        {
            if (!int.TryParse(locationId, out var buildingId))
            {
                return null;
            }

            return await _context.Buildings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId && b.IsActive);
        }

        private async Task<Building?> GetAccessibleBuildingAsync(string locationId, int currentUserId)
        {
            var building = await GetBuildingAsync(locationId);
            if (building == null)
            {
                return null;
            }

            var hasAccess = await HasBuildingAccessAsync(currentUserId, building.BuildingId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have access to this building.");
            }

            return building;
        }

        private async Task<bool> HasBuildingAccessAsync(int currentUserId, int buildingId)
        {
            var accessibleBuildingIds = await GetAccessibleBuildingIdsAsync(currentUserId);
            return accessibleBuildingIds.Contains(buildingId);
        }

        private async Task<HashSet<int>> GetAccessibleBuildingIdsAsync(int currentUserId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId && u.IsActive);

            if (user == null)
            {
                return new HashSet<int>();
            }

            if (user.UserTypeId == UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                var allActive = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.IsActive)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
                return allActive.ToHashSet();
            }

            var explicitPermissions = await _context.UserBuildingPermissions
                .AsNoTracking()
                .Where(p => p.UserId == currentUserId && p.IsActive)
                .Select(p => p.BuildingId)
                .ToListAsync();

            if (explicitPermissions.Any())
            {
                return explicitPermissions.ToHashSet();
            }

            if (user.UserTypeId == UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID)
            {
                var pmOwned = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.IsActive && b.CustomerId == currentUserId)
                    .Select(b => b.BuildingId)
                    .ToListAsync();
                return pmOwned.ToHashSet();
            }

            if (user.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
            {
                var tenantBuildings = await _context.Units
                    .AsNoTracking()
                    .Where(u => u.IsActive && u.TenantId == currentUserId)
                    .Select(u => u.BuildingId)
                    .Distinct()
                    .ToListAsync();
                return tenantBuildings.ToHashSet();
            }

            return new HashSet<int>();
        }

        private async Task<EnergyLocation?> GetEnergyLocationByBuildingIdAsync(int buildingId)
        {
            return await _context.EnergyLocations
                .AsNoTracking()
                .Where(e => e.BuildingId == buildingId)
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private static string ResolveLocationKey(int buildingId, EnergyLocation? energyLocation)
        {
            if (!string.IsNullOrWhiteSpace(energyLocation?.Id))
            {
                return energyLocation.Id;
            }

            return buildingId.ToString();
        }

        private static string ResolveStatus(Building building, EnergyLocation? energy)
        {
            if (!building.IsActive)
            {
                return "offline";
            }

            if (!string.IsNullOrWhiteSpace(energy?.Status))
            {
                return energy.Status;
            }

            if (energy?.LastDataReceived.HasValue == true && energy.LastDataReceived.Value > DateTime.UtcNow.AddMinutes(-10))
            {
                return "online";
            }

            return "active";
        }

        private string? ResolveBucket(EnergyLocation? energyLocation)
        {
            if (!string.IsNullOrWhiteSpace(energyLocation?.InfluxBucket))
            {
                return energyLocation.InfluxBucket;
            }

            return string.IsNullOrWhiteSpace(_influxSettings.DefaultBucket)
                ? null
                : _influxSettings.DefaultBucket;
        }

        private string? ResolveTopicPrefix(EnergyLocation? energyLocation, Building building)
        {
            if (!string.IsNullOrWhiteSpace(energyLocation?.MqttTopicPrefix))
            {
                return energyLocation.MqttTopicPrefix;
            }

            if (string.IsNullOrWhiteSpace(_influxSettings.DefaultTopicPrefix))
            {
                return null;
            }

            var value = _influxSettings.DefaultTopicPrefix
                .Replace("{buildingId}", building.BuildingId.ToString())
                .Replace("{buildingName}", Slugify(building.Name));

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);
            var prevDash = false;
            foreach (var ch in value.ToLowerInvariant())
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                    prevDash = false;
                }
                else if (!prevDash)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }

            return sb.ToString().Trim('-');
        }

        private static EnergyConsumptionDto BuildConsumptionFallback(
            string locationId,
            int baselineKwh,
            decimal baselineCost,
            decimal annualBaseline,
            int current = 0)
        {
            var savings = baselineKwh - current;
            var savingsPercent = baselineKwh > 0 ? Math.Round((double)savings / baselineKwh * 100.0, 2) : 0;
            var costCurrent = Math.Round(current * EffectiveRatePerKwh, 2);
            var costSavings = baselineCost - costCurrent;

            return new EnergyConsumptionDto
            {
                LocationId = locationId,
                Current = current,
                Baseline = baselineKwh,
                Savings = savings,
                SavingsPercent = savingsPercent,
                CostCurrent = costCurrent,
                CostBaseline = baselineCost,
                CostSavings = costSavings,
                AnnualBaseline = annualBaseline,
                AnnualTarget = Math.Round(annualBaseline * 0.85m, 2)
            };
        }

        private static List<HourlyDataDto> BuildEmptyHourly(int monthBaseline)
        {
            var baselinePerHour = monthBaseline > 0 ? Math.Round(monthBaseline / 30d / 24d, 2) : 0;
            var result = new List<HourlyDataDto>(24);

            for (var hour = 0; hour < 24; hour++)
            {
                result.Add(new HourlyDataDto
                {
                    Hour = $"{hour:00}:00",
                    Consumption = 0,
                    Baseline = baselinePerHour,
                    Power = 0,
                    OutdoorTemp = 0
                });
            }

            return result;
        }
    }
}

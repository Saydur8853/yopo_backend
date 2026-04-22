namespace YopoBackend.Modules.Energy.DTOs
{
    public class EnergyLocationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Address { get; set; }
        public string City { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public List<string> Towers { get; set; } = new();
        public int Floors { get; set; }
        public int Basements { get; set; }
        public string? BmsType { get; set; }
        public string? GatewayId { get; set; }
        public int OccupancyPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ConnectedSince { get; set; }
        public DateTime? LastDataReceived { get; set; }
    }

    public class BuildingOverviewDto
    {
        public string LocationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Status { get; set; } = "offline";
        public int OccupancyPercent { get; set; }
        public int TotalUnits { get; set; }
        public int DewaMeterCount { get; set; }
        public int ActiveAlerts { get; set; }
        public int CriticalAlerts { get; set; }
    }

    public class EnergyConsumptionDto
    {
        public string LocationId { get; set; } = string.Empty;
        public int Current { get; set; }
        public int Baseline { get; set; }
        public int Savings { get; set; }
        public double SavingsPercent { get; set; }
        public decimal CostCurrent { get; set; }
        public decimal CostBaseline { get; set; }
        public decimal CostSavings { get; set; }
        public decimal AnnualBaseline { get; set; }
        public decimal AnnualTarget { get; set; }
    }

    public class HourlyDataDto
    {
        public string Hour { get; set; } = string.Empty;
        public double Consumption { get; set; }
        public double Baseline { get; set; }
        public double Power { get; set; }
        public double OutdoorTemp { get; set; }
    }

    public class MonthlyDataDto
    {
        public string Month { get; set; } = string.Empty;
        public int Kwh { get; set; }
        public decimal Cost { get; set; }
        public int BaselineKwh { get; set; }
        public decimal BaselineCost { get; set; }
        public int SavingsKwh { get; set; }
        public double SavingsPct { get; set; }
    }

    public class DewaMeterDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string PremiseLabel { get; set; } = string.Empty;
        public string? MeterNumber { get; set; }
        public decimal? Mf { get; set; }
        public string? CtRatio { get; set; }
        public string? Description { get; set; }
        public int? LastReading { get; set; }
        public int? LastKwh { get; set; }
        public decimal? LastAed { get; set; }
        public bool HasWater { get; set; }
        public decimal? LastWaterAed { get; set; }
    }

    public class EnergyTopicReadingDto
    {
        public string Topic { get; set; } = string.Empty;
        public string? Site { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Point { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class EnergyTopicLiveResponseDto
    {
        public string LocationId { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public string? TopicPrefix { get; set; }
        public int TotalTopics { get; set; }
        public List<EnergyTopicReadingDto> Readings { get; set; } = new();
    }
}

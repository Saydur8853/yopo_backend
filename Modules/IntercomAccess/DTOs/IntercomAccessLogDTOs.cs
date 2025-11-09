namespace YopoBackend.Modules.IntercomAccess.DTOs
{
    public class AccessLogDTO
    {
        public long Id { get; set; }
        public int IntercomId { get; set; }
        public int? UserId { get; set; }
        public string CredentialType { get; set; } = string.Empty;
        public int? CredentialRefId { get; set; }
        public bool IsSuccess { get; set; }
        public string? Reason { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class TemporaryPinUsageDTO
    {
        public long Id { get; set; }
        public int TemporaryPinId { get; set; }
        public DateTime UsedAt { get; set; }
        public string? UsedFromIp { get; set; }
        public string? DeviceInfo { get; set; }
    }
}
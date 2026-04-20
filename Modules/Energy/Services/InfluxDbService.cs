using InfluxDB.Client;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using YopoBackend.Modules.Energy.DTOs;

namespace YopoBackend.Modules.Energy.Services
{
    public class InfluxDbService : IInfluxDbService, IDisposable
    {
        private readonly InfluxDBClient? _client;
        private readonly InfluxDbSettings _settings;

        public InfluxDbService(IOptions<InfluxDbSettings> options)
        {
            _settings = options.Value;
            if (!string.IsNullOrWhiteSpace(_settings.Url) &&
                !string.IsNullOrWhiteSpace(_settings.Token) &&
                !string.IsNullOrWhiteSpace(_settings.Org))
            {
                _client = new InfluxDBClient(_settings.Url, _settings.Token);
            }
        }

        public async Task<double> GetCurrentPowerKwAsync(string bucket, string topicPrefix)
        {
            try
            {
                if (_client == null)
                {
                    return 0;
                }

                var q = $$"""
from(bucket: "{{bucket}}")
  |> range(start: -5m)
  |> filter(fn: (r) => r["_field"] == "value")
  |> filter(fn: (r) => r["topic"] =~ /{{BuildTopicRegex(topicPrefix)}}\/.*power_kw$/)
  |> group(columns: ["topic"])
  |> last()
  |> group()
  |> sum(column: "_value")
""";

                var result = await QuerySingleDoubleAsync(q);
                return result ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<double> GetCurrentMonthConsumptionKwhAsync(string bucket, string topicPrefix, DateTime utcNow)
        {
            try
            {
                if (_client == null)
                {
                    return 0;
                }

                var start = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var stop = start.AddMonths(1);

                var q = $$"""
from(bucket: "{{bucket}}")
  |> range(start: {{start:yyyy-MM-ddTHH:mm:ssZ}}, stop: {{stop:yyyy-MM-ddTHH:mm:ssZ}})
  |> filter(fn: (r) => r["_field"] == "value")
  |> filter(fn: (r) => r["topic"] =~ /{{BuildTopicRegex(topicPrefix)}}\/.*power_kw$/)
  |> aggregateWindow(every: 1h, fn: mean, createEmpty: false)
  |> sum(column: "_value")
""";

                var result = await QuerySingleDoubleAsync(q);
                return result ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Dictionary<int, double>> GetHourlyAveragePowerKwAsync(string bucket, string topicPrefix, DateTime dateUtc)
        {
            var response = new Dictionary<int, double>();

            try
            {
                if (_client == null)
                {
                    return response;
                }

                var start = new DateTime(dateUtc.Year, dateUtc.Month, dateUtc.Day, 0, 0, 0, DateTimeKind.Utc);
                var stop = start.AddDays(1);

                var q = $$"""
from(bucket: "{{bucket}}")
  |> range(start: {{start:yyyy-MM-ddTHH:mm:ssZ}}, stop: {{stop:yyyy-MM-ddTHH:mm:ssZ}})
  |> filter(fn: (r) => r["_field"] == "value")
  |> filter(fn: (r) => r["topic"] =~ /{{BuildTopicRegex(topicPrefix)}}\/.*power_kw$/)
  |> aggregateWindow(every: 1h, fn: mean, createEmpty: false)
  |> group(columns:["_time"])
  |> sum(column: "_value")
  |> sort(columns:["_time"], desc: false)
""";

                if (_client == null)
                {
                    return response;
                }

                var tables = await _client.GetQueryApi().QueryAsync(q, _settings.Org);
                foreach (var table in tables)
                {
                    foreach (var record in table.Records)
                    {
                        if (record.GetTimeInDateTime() is not DateTime dt)
                        {
                            continue;
                        }

                        var value = ConvertToDouble(record.GetValue());
                        response[dt.Hour] = value;
                    }
                }

                return response;
            }
            catch
            {
                return response;
            }
        }

        private async Task<double?> QuerySingleDoubleAsync(string flux)
        {
            if (_client == null)
            {
                return null;
            }

            var tables = await _client.GetQueryApi().QueryAsync(flux, _settings.Org);
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    return ConvertToDouble(record.GetValue());
                }
            }

            return null;
        }

        private static string BuildTopicRegex(string topicPrefix)
        {
            var escaped = Regex.Escape(topicPrefix);
            return escaped.Replace("/", "\\/");
        }

        private static double ConvertToDouble(object? value)
        {
            if (value == null)
            {
                return 0;
            }

            return value switch
            {
                double d => d,
                float f => f,
                decimal m => (double)m,
                int i => i,
                long l => l,
                _ => double.TryParse(value.ToString(), out var parsed) ? parsed : 0
            };
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}

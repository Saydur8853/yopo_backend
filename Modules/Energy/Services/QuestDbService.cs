using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;
using YopoBackend.Modules.Energy.DTOs;

namespace YopoBackend.Modules.Energy.Services
{
    public class QuestDbService : IQuestDbService
    {
        private readonly QuestDbSettings _settings;
        private readonly string? _connectionString;

        public QuestDbService(IOptions<QuestDbSettings> options)
        {
            _settings = options.Value;

            if (!string.IsNullOrWhiteSpace(_settings.Host) &&
                !string.IsNullOrWhiteSpace(_settings.Username))
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = _settings.Host,
                    Port = _settings.Port > 0 ? _settings.Port : 8812,
                    Database = string.IsNullOrWhiteSpace(_settings.Database) ? "qdb" : _settings.Database,
                    Username = _settings.Username,
                    Password = _settings.Password,
                    SslMode = SslMode.Disable,
                    Timeout = 5,
                    CommandTimeout = 10
                };
                _connectionString = builder.ConnectionString;
            }
        }

        public async Task<double> GetCurrentPowerKwAsync(int buildingId, string? topicPrefix = null)
        {
            if (buildingId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
            {
                return 0;
            }

            if (!string.IsNullOrWhiteSpace(topicPrefix))
            {
                var mqttValue = await GetCurrentPowerFromMqttConsumerAsync(buildingId, topicPrefix);
                if (mqttValue > 0)
                {
                    return mqttValue;
                }
            }

            var sql = $@"
SELECT COALESCE(SUM(value), 0) AS total_kw
FROM (
    SELECT SensorID, Value
    FROM {SanitizeIdentifier(_settings.TableName)}
    WHERE BuildingID = @buildingId
      AND SensorID LIKE @powerPattern
    LATEST ON timestamp PARTITION BY SensorID
) t;";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@buildingId", NpgsqlDbType.Integer, buildingId);
                cmd.Parameters.AddWithValue("@powerPattern", NpgsqlDbType.Varchar, ResolvePowerPattern());

                var value = await cmd.ExecuteScalarAsync();
                return ConvertToDouble(value);
            }
            catch
            {
                // Fallback for existing mqtt_consumer schema
                return await GetCurrentPowerFromMqttConsumerAsync(buildingId, topicPrefix);
            }
        }

        public async Task<double> GetCurrentMonthConsumptionKwhAsync(int buildingId, DateTime utcNow, string? topicPrefix = null)
        {
            var hourly = await QueryHourlyAverageByUtcRangeAsync(
                buildingId,
                new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
                topicPrefix);

            return hourly.Values.Sum();
        }

        public async Task<Dictionary<int, double>> GetHourlyAveragePowerKwAsync(int buildingId, DateTime dateUtc, string? topicPrefix = null)
        {
            var start = new DateTime(dateUtc.Year, dateUtc.Month, dateUtc.Day, 0, 0, 0, DateTimeKind.Utc);
            var stop = start.AddDays(1);

            var hourly = await QueryHourlyAverageByUtcRangeAsync(buildingId, start, stop, topicPrefix);
            return hourly.ToDictionary(x => x.Key.Hour, x => x.Value);
        }

        public async Task<List<EnergyTopicReadingDto>> GetLatestTopicReadingsAsync(int buildingId, string? topicPrefix = null, int limit = 20)
        {
            var result = new List<EnergyTopicReadingDto>();
            if (buildingId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
            {
                return result;
            }

            var prefixLike = ResolveTopicPrefixLike(topicPrefix, buildingId);
            if (string.IsNullOrWhiteSpace(prefixLike))
            {
                return result;
            }

            var safeLimit = Math.Clamp(limit, 1, 200);
            var sql = $@"
SELECT topic, value, timestamp
FROM {SanitizeIdentifier(_settings.MqttConsumerTableName)}
WHERE topic LIKE '{EscapeSqlLiteral(prefixLike)}'
  AND topic LIKE '{EscapeSqlLiteral(ResolveMqttTopicPattern())}'
LATEST ON timestamp PARTITION BY topic
LIMIT {safeLimit};";

            try
            {
                var endpoint = $"http://{_settings.Host}:9000/exec?query={Uri.EscapeDataString(sql)}";
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                using var response = await http.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("dataset", out var dataset) || dataset.ValueKind != JsonValueKind.Array)
                {
                    return result;
                }

                foreach (var row in dataset.EnumerateArray())
                {
                    if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() < 3)
                    {
                        continue;
                    }

                    var topic = row[0].ValueKind == JsonValueKind.String ? row[0].GetString() ?? string.Empty : string.Empty;
                    if (string.IsNullOrWhiteSpace(topic))
                    {
                        continue;
                    }

                    var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    var site = parts.Length > 1 ? parts[1] : null;
                    var subcategory = parts.Length > 2 ? parts[2] : null;
                    var category = parts.Length > 3 ? parts[3] : null;
                    var point = parts.Length > 4 ? parts[4] : null;

                    var value = 0d;
                    if (row[1].ValueKind == JsonValueKind.Number)
                    {
                        value = row[1].GetDouble();
                    }
                    else if (row[1].ValueKind == JsonValueKind.String && double.TryParse(row[1].GetString(), out var parsed))
                    {
                        value = parsed;
                    }

                    var ts = DateTime.UtcNow;
                    if (row[2].ValueKind == JsonValueKind.String && DateTime.TryParse(row[2].GetString(), out var parsedTs))
                    {
                        ts = DateTime.SpecifyKind(parsedTs, DateTimeKind.Utc);
                    }

                    result.Add(new EnergyTopicReadingDto
                    {
                        Topic = topic,
                        Site = site,
                        Category = category,
                        Subcategory = subcategory,
                        Point = point,
                        Value = value,
                        Timestamp = ts
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Energy][QuestDbService] GetLatestTopicReadingsAsync failed. buildingId={buildingId}, prefix={prefixLike}, error={ex.Message}");
                return new List<EnergyTopicReadingDto>();
            }

            return result;
        }

        private async Task<Dictionary<DateTime, double>> QueryHourlyAverageByUtcRangeAsync(int buildingId, DateTime startUtc, DateTime stopUtc, string? topicPrefix)
        {
            var result = new Dictionary<DateTime, double>();
            if (buildingId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(topicPrefix))
            {
                var mqttResult = await QueryHourlyAverageFromMqttConsumerAsync(startUtc, stopUtc, topicPrefix);
                if (mqttResult.Count > 0)
                {
                    return mqttResult;
                }
            }

            var sql = $@"
SELECT date_trunc('hour', timestamp) AS ts_hour,
       SensorID,
       AVG(Value) AS avg_kw
FROM {SanitizeIdentifier(_settings.TableName)}
WHERE BuildingID = @buildingId
  AND SensorID LIKE @powerPattern
  AND timestamp >= @startUtc
  AND timestamp < @stopUtc
GROUP BY ts_hour, SensorID
ORDER BY ts_hour;";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@buildingId", NpgsqlDbType.Integer, buildingId);
                cmd.Parameters.AddWithValue("@powerPattern", NpgsqlDbType.Varchar, ResolvePowerPattern());
                cmd.Parameters.AddWithValue("@startUtc", NpgsqlDbType.TimestampTz, startUtc);
                cmd.Parameters.AddWithValue("@stopUtc", NpgsqlDbType.TimestampTz, stopUtc);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.IsDBNull(0))
                    {
                        continue;
                    }

                    var hour = DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc);
                    var avgKw = reader.IsDBNull(2) ? 0 : ConvertToDouble(reader.GetValue(2));

                    if (result.ContainsKey(hour))
                    {
                        result[hour] += avgKw;
                    }
                    else
                    {
                        result[hour] = avgKw;
                    }
                }

                return result;
            }
            catch
            {
                return await QueryHourlyAverageFromMqttConsumerAsync(startUtc, stopUtc, topicPrefix);
            }
        }

        private async Task<double> GetCurrentPowerFromMqttConsumerAsync(int buildingId, string? topicPrefix)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return 0;
            }

            var prefixLike = ResolveTopicPrefixLike(topicPrefix, buildingId);
            if (string.IsNullOrWhiteSpace(prefixLike))
            {
                return 0;
            }

            var sql = $@"
SELECT COALESCE(SUM(value), 0) AS total_kw
FROM (
    SELECT topic, value
    FROM {SanitizeIdentifier(_settings.MqttConsumerTableName)}
    WHERE topic LIKE @topicPrefixLike
      AND topic LIKE @topicLikePattern
      AND point LIKE @pointLikePattern
    LATEST ON timestamp PARTITION BY topic
) t;";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@topicPrefixLike", NpgsqlDbType.Varchar, prefixLike);
                cmd.Parameters.AddWithValue("@topicLikePattern", NpgsqlDbType.Varchar, ResolveMqttTopicPattern());
                cmd.Parameters.AddWithValue("@pointLikePattern", NpgsqlDbType.Varchar, ResolveMqttPointPattern());

                var value = await cmd.ExecuteScalarAsync();
                return ConvertToDouble(value);
            }
            catch
            {
                return 0;
            }
        }

        private async Task<Dictionary<DateTime, double>> QueryHourlyAverageFromMqttConsumerAsync(
            DateTime startUtc,
            DateTime stopUtc,
            string? topicPrefix)
        {
            var result = new Dictionary<DateTime, double>();
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return result;
            }

            // If we cannot resolve prefix, skip fallback to avoid cross-building leakage.
            var prefixLike = ResolveTopicPrefixLike(topicPrefix, null);
            if (string.IsNullOrWhiteSpace(prefixLike))
            {
                return result;
            }

            var sql = $@"
SELECT date_trunc('hour', timestamp) AS ts_hour,
       topic,
       AVG(value) AS avg_v
FROM {SanitizeIdentifier(_settings.MqttConsumerTableName)}
WHERE topic LIKE @topicPrefixLike
  AND topic LIKE @topicLikePattern
  AND point LIKE @pointLikePattern
  AND timestamp >= @startUtc
  AND timestamp < @stopUtc
GROUP BY ts_hour, topic
ORDER BY ts_hour;";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@topicPrefixLike", NpgsqlDbType.Varchar, prefixLike);
                cmd.Parameters.AddWithValue("@topicLikePattern", NpgsqlDbType.Varchar, ResolveMqttTopicPattern());
                cmd.Parameters.AddWithValue("@pointLikePattern", NpgsqlDbType.Varchar, ResolveMqttPointPattern());
                cmd.Parameters.AddWithValue("@startUtc", NpgsqlDbType.TimestampTz, startUtc);
                cmd.Parameters.AddWithValue("@stopUtc", NpgsqlDbType.TimestampTz, stopUtc);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.IsDBNull(0))
                    {
                        continue;
                    }

                    var hour = DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc);
                    var avg = reader.IsDBNull(2) ? 0 : ConvertToDouble(reader.GetValue(2));
                    if (result.ContainsKey(hour))
                    {
                        result[hour] += avg;
                    }
                    else
                    {
                        result[hour] = avg;
                    }
                }

                return result;
            }
            catch
            {
                return new Dictionary<DateTime, double>();
            }
        }

        private string ResolvePowerPattern()
        {
            return string.IsNullOrWhiteSpace(_settings.PowerSensorLikePattern)
                ? "%power_kw"
                : _settings.PowerSensorLikePattern;
        }

        private string ResolveMqttPointPattern()
        {
            return string.IsNullOrWhiteSpace(_settings.MqttPointLikePattern)
                ? "%"
                : _settings.MqttPointLikePattern;
        }

        private string ResolveMqttTopicPattern()
        {
            return string.IsNullOrWhiteSpace(_settings.MqttTopicLikePattern)
                ? "%"
                : _settings.MqttTopicLikePattern;
        }

        private string? ResolveTopicPrefixLike(string? topicPrefix, int? buildingId)
        {
            var prefix = topicPrefix?.Trim();
            if (string.IsNullOrWhiteSpace(prefix) && buildingId.HasValue && buildingId.Value > 0)
            {
                var template = string.IsNullOrWhiteSpace(_settings.DefaultTopicPrefixTemplate)
                    ? string.Empty
                    : _settings.DefaultTopicPrefixTemplate;
                if (!string.IsNullOrWhiteSpace(template))
                {
                    prefix = template.Replace("{buildingId}", buildingId.Value.ToString());
                }
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return null;
            }

            prefix = prefix.TrimEnd('/');
            return $"{prefix}/%";
        }

        private static string SanitizeIdentifier(string input)
        {
            var id = string.IsNullOrWhiteSpace(input) ? "bms_readings" : input.Trim();
            foreach (var ch in id)
            {
                if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                {
                    return "bms_readings";
                }
            }
            return id;
        }

        private static double ConvertToDouble(object? value)
        {
            if (value == null || value == DBNull.Value)
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

        private static string EscapeSqlLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}

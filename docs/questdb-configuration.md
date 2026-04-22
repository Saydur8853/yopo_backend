# QuestDB Configuration Guide (YOPO Energy API)

This backend has been migrated from InfluxDB to QuestDB for live energy time-series reads.

## 1) What Changed

- Removed runtime use of InfluxDB client.
- Added QuestDB integration over PostgreSQL wire protocol (`Npgsql`).
- Energy APIs now read live time-series from QuestDB table `bms_readings` using `BuildingID`.
- SQL (MySQL) tables are still used for:
  - buildings/locations metadata
  - DEWA bills/meters
  - baseline/cost calculations

## 2) Required `.env` Settings

Set these in `.env`:

```env
QuestDb__Host=5.189.144.27
QuestDb__Port=8812
QuestDb__Database=qdb
QuestDb__Username=yopo
QuestDb__Password=YopoQuest2026!
QuestDb__TableName=bms_readings
QuestDb__PowerSensorLikePattern=%power_kw
QuestDb__MqttConsumerTableName=mqtt_consumer
QuestDb__MqttPointLikePattern=value
QuestDb__MqttTopicLikePattern=%
QuestDb__DefaultTopicPrefixTemplate=yopo/{shortName}
```

Notes:
- `QuestDb__PowerSensorLikePattern` is used to pick power sensors (default: `%power_kw`).
- Backend now supports two shapes:
  - Preferred: normalized `bms_readings` (`BuildingID`, `SensorID`, `Value`, `timestamp`)
  - Compatibility: existing `mqtt_consumer` (`topic`, `point`, `value`, `timestamp`)
- For `mqtt_consumer`, building filtering is done by prefix from `Energy_Locations.MqttTopicPrefix` (example: `yopo/sparkle`).
- If QuestDB is unreachable or no matching data exists, API returns fallback `0` values (no crash).

## 3) Expected QuestDB Table

QuestDB table used by the API:

```sql
CREATE TABLE bms_readings (
  timestamp TIMESTAMP,
  BuildingID SYMBOL,
  SensorID SYMBOL,
  Value DOUBLE
) TIMESTAMP(timestamp) PARTITION BY DAY;
```

Minimum required columns for this backend:
- `timestamp`
- `BuildingID`
- `SensorID`
- `Value`

## 4) Quick Validation

Run backend:

```bash
dotnet run
```

Then test:
1. Login and get token (`/api/users/login`).
2. Call:
   - `GET /api/energy/locations/{locationId}/energy/current-power`
   - `GET /api/energy/locations/{locationId}/energy/hourly?date=YYYY-MM-DD`
   - `GET /api/energy/locations/{locationId}/energy/consumption`

If live data exists in QuestDB, these endpoints return non-zero live values.

## 5) Optional SQL Mapping Table (Team Instruction)

If your pipeline uses explicit sensor mapping, create this in MySQL:

```sql
CREATE TABLE BMS_Sensor_Map (
    SensorID VARCHAR(50) PRIMARY KEY,
    BuildingID INT,
    LocationID INT,
    UnitType VARCHAR(20),
    Description VARCHAR(100)
);
```

Current backend implementation queries QuestDB by `BuildingID` + sensor pattern directly.
If you want strict sensor filtering via `BMS_Sensor_Map`, that can be added in a follow-up patch.

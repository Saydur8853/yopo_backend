# 21-Energy API Usage Guide (YOPO Backend)

This document explains how to use the **Energy module APIs** currently available under Swagger tag `21-Energy` in the YOPO backend.

## 1. Base URL

Local example (running from this project):

```text
http://127.0.0.1:5207
```

Swagger:

```text
http://127.0.0.1:5207/swagger
```

## 2. Current Energy Endpoints (Phase 1)

- `GET /api/energy/locations`
- `GET /api/energy/locations/{locationId}/overview`
- `GET /api/energy/locations/{locationId}/dewa-meters`
- `GET /api/energy/locations/{locationId}/energy/consumption`
- `GET /api/energy/locations/{locationId}/energy/current-power`
- `GET /api/energy/locations/{locationId}/energy/hourly?date=YYYY-MM-DD`
- `GET /api/energy/locations/{locationId}/energy/monthly`

Location mapping:

- `locationId` is the YOPO `BuildingId` (string in route, numeric value), for example `1`.

## 3. Quick Start (cURL)

## 3.1 Get locations

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations"
```

Example response:

```json
[
  {
    "id": "sparkle-towers",
    "name": "Sparkle Towers",
    "shortName": "Sparkle",
    "address": "Dubai Marina, Plot 392",
    "city": "Dubai",
    "totalUnits": 392,
    "towers": ["Tower A", "Tower B"],
    "floors": 29,
    "basements": 3,
    "bmsType": "Johnson Controls Metasys",
    "gatewayId": "yopo-gw-sparkle-01",
    "occupancyPercent": 15,
    "status": "pilot",
    "connectedSince": "2026-04-20T00:00:00Z",
    "lastDataReceived": "2026-04-18T14:30:00Z"
  }
]
```

## 3.2 Get building overview

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/overview"
```

Example response:

```json
{
  "locationId": "1",
  "name": "Sparkle Towers",
  "address": "Dubai Marina, Plot 392",
  "lastUpdated": "2026-04-18T14:30:00Z",
  "status": "offline",
  "occupancyPercent": 15,
  "totalUnits": 392,
  "dewaMeterCount": 2,
  "activeAlerts": 0,
  "criticalAlerts": 0
}
```

## 3.3 Get DEWA meters

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/dewa-meters"
```

Example response:

```json
[
  {
    "accountNumber": "3001970125",
    "premiseLabel": "LVP-1",
    "meterNumber": "909653P",
    "mf": 487.20,
    "ctRatio": "2400/5",
    "description": "Common services + water supply",
    "lastReading": 5300,
    "lastKwh": 16173,
    "lastAed": 43510.63,
    "hasWater": true,
    "lastWaterAed": 28577.20
  },
  {
    "accountNumber": "3001970071",
    "premiseLabel": "LVP-6",
    "meterNumber": "909606P",
    "mf": 487.20,
    "ctRatio": "2400/5",
    "description": "Main chiller plant + primary common areas",
    "lastReading": 25000,
    "lastKwh": 86722,
    "lastAed": 39477.39,
    "hasWater": false,
    "lastWaterAed": null
  }
]
```

## 3.4 Get energy consumption summary

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/energy/consumption"
```

Example response shape:

```json
{
  "locationId": "1",
  "current": 0,
  "baseline": 166691,
  "savings": 166691,
  "savingsPercent": 100.0,
  "costCurrent": 0.00,
  "costBaseline": 75834.97,
  "costSavings": 75834.97,
  "annualBaseline": 158822.99,
  "annualTarget": 134999.54
}
```

Notes:
- `current` comes from QuestDB; if QuestDB is not configured/reachable, it returns `0`.
- API still returns `200` with fallback values (no 500 from QuestDB issues).

## 3.5 Get current power (kW)

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/energy/current-power"
```

Example response:

```json
0
```

## 3.6 Get hourly energy/power

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/energy/hourly?date=2026-04-18"
```

Example response shape (24 items):

```json
[
  {
    "hour": "00:00",
    "consumption": 0,
    "baseline": 231.52,
    "power": 0,
    "outdoorTemp": 0
  },
  {
    "hour": "01:00",
    "consumption": 0,
    "baseline": 231.52,
    "power": 0,
    "outdoorTemp": 0
  }
]
```

## 3.7 Get monthly energy (baseline year aggregation)

```bash
curl -X GET "http://127.0.0.1:5207/api/energy/locations/1/energy/monthly"
```

Example response:

```json
[
  {
    "month": "Feb",
    "kwh": 166691,
    "cost": 75834.97,
    "baselineKwh": 166691,
    "baselineCost": 75834.97,
    "savingsKwh": 0,
    "savingsPct": 0
  },
  {
    "month": "Apr",
    "kwh": 102895,
    "cost": 82988.02,
    "baselineKwh": 102895,
    "baselineCost": 82988.02,
    "savingsKwh": 0,
    "savingsPct": 0
  }
]
```

## 4. Error Handling

Possible status codes:

- `200 OK`: successful request
- `404 Not Found`: invalid `locationId` for endpoints that require location lookup

Example 404:

```json
{
  "message": "Location 'unknown-site' not found."
}
```

## 5. QuestDB Configuration (for live values)

Recommended: set credentials in `.env` (already loaded by `DotNetEnv` in this project).

```env
QuestDb__Host=5.189.144.27
QuestDb__Port=8812
QuestDb__Database=qdb
QuestDb__Username=yopo
QuestDb__Password=your-password
QuestDb__TableName=bms_readings
QuestDb__PowerSensorLikePattern=%power_kw
```

If QuestDB is unavailable, Energy APIs continue working with SQL-based + fallback values.

## 6. Suggested Test Flow (YOPO System)

1. Call `/api/energy/locations` and take `id`.
2. Call `/overview` to show dashboard header data.
3. Call `/energy/consumption` and `/energy/current-power` for top KPI cards.
4. Call `/energy/hourly` for chart data.
5. Call `/energy/monthly` for report tab monthly bars.
6. Call `/dewa-meters` for meter breakdown table.

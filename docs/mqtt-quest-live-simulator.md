# MQTT -> QuestDB Live Simulator

Use this to generate virtual fluctuating energy data and verify end-to-end flow:

`MQTT publish -> QuestDB mqtt_consumer -> YOPO Energy APIs`.

If your existing pipeline is not writing MQTT into QuestDB yet, run the bridge script in section 3.

## 1) Install dependency

```bash
pip install paho-mqtt
```

## 2) Run simulator

Script path:

`Scripts/simulate_energy_mqtt.py`

Run:

```bash
python Scripts/simulate_energy_mqtt.py
```

It publishes every 2 seconds:

- `yopo/sparkle/test/test/value` (building 1)
- `yopo/anabil/test/test/value` (building 2)

## 3) Verify QuestDB receives rows

Open QuestDB console and run:

```sql
SELECT timestamp, topic, point, value
FROM mqtt_consumer
WHERE topic LIKE 'yopo/sparkle/%' OR topic LIKE 'yopo/anabil/%'
ORDER BY timestamp DESC
LIMIT 50;
```

## 3.1 Optional: Run local MQTT -> QuestDB bridge

If rows are not increasing in `mqtt_consumer`, run:

```bash
python Scripts/mqtt_to_quest_bridge.py
```

This subscribes to `yopo/#` on MQTT and writes each numeric payload to QuestDB ILP port `9009` into `mqtt_consumer`.

## 4) Verify API shows changing values

Login and call:

- Sia (`buildingId=1`): `GET /api/energy/locations/1/energy/current-power`
- Hridoy (`buildingId=2`): `GET /api/energy/locations/2/energy/current-power`

Expected:

- Values change over time while simulator is running.
- Cross-building access remains blocked by backend authorization (`403`).

## 5) Stop simulator

Press `Ctrl + C`.

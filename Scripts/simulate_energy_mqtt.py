import math
import random
import time

import paho.mqtt.client as mqtt


BROKER = "5.189.144.27"
PORT = 1883
USERNAME = "yopo"
PASSWORD = "YopoMQTT2026!"
PUBLISH_INTERVAL_SECONDS = 2

BUILDINGS = ["sparkle", "anabil"]


def publish_building_metrics(client: mqtt.Client, site: str, kw: float, tick: int) -> None:
    energy_kwh_total = 12000 + tick * (kw / 1800.0)
    chilled_water_temp = 6.8 + 0.6 * math.sin(tick / 9.0) + random.uniform(-0.2, 0.2)
    return_water_temp = chilled_water_temp + 3.5 + random.uniform(-0.3, 0.3)
    ambient_temp = 24.0 + 2.0 * math.sin(tick / 12.0) + random.uniform(-0.4, 0.4)
    co2_ppm = 580 + 70 * math.sin(tick / 7.0) + random.uniform(-20, 20)

    # "value" point is kept for current power endpoint compatibility.
    messages = {
        f"yopo/{site}/test/test/value": kw,
        f"yopo/{site}/electrical/main/power_kw": kw,
        f"yopo/{site}/electrical/main/energy_kwh_total": energy_kwh_total,
        f"yopo/{site}/hvac/chw/supply_temp_c": chilled_water_temp,
        f"yopo/{site}/hvac/chw/return_temp_c": return_water_temp,
        f"yopo/{site}/environment/lobby/temp_c": ambient_temp,
        f"yopo/{site}/environment/parking/co2_ppm": co2_ppm
    }

    for topic, value in messages.items():
        client.publish(topic, f"{value:.2f}")


def main() -> None:
    client = mqtt.Client()
    client.username_pw_set(USERNAME, PASSWORD)
    client.connect(BROKER, PORT, 60)
    client.loop_start()

    tick = 0
    print("Publishing simulated MQTT telemetry. Press Ctrl+C to stop.")
    try:
        while True:
            # Simulated live kW-like values with fluctuation
            sparkle_kw = 42 + 8 * math.sin(tick / 8.0) + random.uniform(-1.5, 1.5)
            anabil_kw = 30 + 6 * math.cos(tick / 10.0) + random.uniform(-1.0, 1.0)

            publish_building_metrics(client, BUILDINGS[0], sparkle_kw, tick)
            publish_building_metrics(client, BUILDINGS[1], anabil_kw, tick)

            print(
                f"[{time.strftime('%H:%M:%S')}] "
                f"yopo/sparkle/* power={sparkle_kw:.2f} kW, "
                f"yopo/anabil/* power={anabil_kw:.2f} kW"
            )
            tick += 1
            time.sleep(PUBLISH_INTERVAL_SECONDS)
    except KeyboardInterrupt:
        print("\nStopped.")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()

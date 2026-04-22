import os
import signal
import socket
import sys
import threading
import time

import paho.mqtt.client as mqtt


MQTT_HOST = os.getenv("Mqtt__Host", "5.189.144.27")
MQTT_PORT = int(os.getenv("Mqtt__Port", "1883"))
MQTT_USERNAME = os.getenv("Mqtt__Username", "yopo")
MQTT_PASSWORD = os.getenv("Mqtt__Password", "YopoMQTT2026!")
MQTT_TOPIC_FILTER = os.getenv("Bridge__MqttTopicFilter", "yopo/#")

QUEST_HOST = os.getenv("QuestDb__Host", "5.189.144.27")
QUEST_ILP_PORT = int(os.getenv("QuestDb__IlpPort", "9009"))
QUEST_MEASUREMENT = os.getenv("QuestDb__MqttConsumerTableName", "mqtt_consumer")


running = True
sock_lock = threading.Lock()
sock = None


def parse_topic(topic: str):
    # Expected style: yopo/<site>/<subcategory>/<category>/<point>
    parts = topic.split("/")
    site = parts[1] if len(parts) > 1 else "unknown"
    subcategory = parts[2] if len(parts) > 2 else "unknown"
    category = parts[3] if len(parts) > 3 else "unknown"
    point = parts[4] if len(parts) > 4 else "value"
    return site, subcategory, category, point


def ilp_escape_tag(v: str) -> str:
    return v.replace("\\", "\\\\").replace(",", "\\,").replace(" ", "\\ ").replace("=", "\\=")


def get_socket():
    global sock
    with sock_lock:
        if sock is not None:
            return sock
        s = socket.create_connection((QUEST_HOST, QUEST_ILP_PORT), timeout=5)
        sock = s
        return sock


def send_line(line: str):
    global sock
    data = (line + "\n").encode("utf-8")
    try:
        s = get_socket()
        s.sendall(data)
    except Exception:
        with sock_lock:
            if sock is not None:
                try:
                    sock.close()
                except Exception:
                    pass
                sock = None
        # one retry
        s = get_socket()
        s.sendall(data)


def on_message(_client, _userdata, msg):
    payload_raw = msg.payload.decode("utf-8", errors="ignore").strip()
    try:
        value = float(payload_raw)
    except ValueError:
        return

    topic = msg.topic
    site, subcategory, category, point = parse_topic(topic)
    host = socket.gethostname()[:40]

    tags = ",".join(
        [
            f"category={ilp_escape_tag(category)}",
            f"host={ilp_escape_tag(host)}",
            f"point={ilp_escape_tag(point)}",
            f"site={ilp_escape_tag(site)}",
            f"subcategory={ilp_escape_tag(subcategory)}",
            f"topic={ilp_escape_tag(topic)}",
        ]
    )
    line = f"{QUEST_MEASUREMENT},{tags} value={value}"
    send_line(line)
    print(f"FORWARDED {topic}={value}")


def shutdown(*_args):
    global running
    running = False


def main():
    signal.signal(signal.SIGINT, shutdown)
    signal.signal(signal.SIGTERM, shutdown)

    client = mqtt.Client()
    client.username_pw_set(MQTT_USERNAME, MQTT_PASSWORD)
    client.on_message = on_message

    print(
        f"Starting MQTT->Quest bridge | MQTT {MQTT_HOST}:{MQTT_PORT} "
        f"filter={MQTT_TOPIC_FILTER} | Quest ILP {QUEST_HOST}:{QUEST_ILP_PORT}"
    )

    client.connect(MQTT_HOST, MQTT_PORT, 60)
    client.subscribe(MQTT_TOPIC_FILTER)
    client.loop_start()

    try:
        while running:
            time.sleep(0.5)
    finally:
        client.loop_stop()
        client.disconnect()
        with sock_lock:
            if sock is not None:
                try:
                    sock.close()
                except Exception:
                    pass
        print("Bridge stopped.")


if __name__ == "__main__":
    main()

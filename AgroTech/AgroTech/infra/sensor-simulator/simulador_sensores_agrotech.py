import json
import math
import os
import random
import socket
import time
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone

import paho.mqtt.client as mqtt


AIO_USERNAME = os.getenv("AIO_USERNAME", "")
AIO_KEY = os.getenv("AIO_KEY", "")
AIO_FEED_KEY = os.getenv("AIO_FEED_KEY", "sensores-agrotech")

MQTT_HOST = os.getenv("MQTT_HOST", "io.adafruit.com")
MQTT_PORT = int(os.getenv("MQTT_PORT", "1883"))

PUBLISH_INTERVAL_SECONDS = int(os.getenv("PUBLISH_INTERVAL_SECONDS", "30"))
SIMULATION_STEP_MINUTES = int(os.getenv("SIMULATION_STEP_MINUTES", "5"))

TOPIC = f"{AIO_USERNAME}/feeds/{AIO_FEED_KEY}"


@dataclass
class FieldState:
    sim_time: datetime
    temperatura_ar: float = 24.0
    temperatura_solo: float = 22.0
    umidade_ar: float = 65.0
    umidade_solo: float = 55.0
    ph_solo: float = 6.2
    luminosidade: float = 0.0
    velocidade_vento: float = 3.5
    chuva: float = 0.0
    cloud_cover: float = 0.2
    rain_steps_remaining: int = 0
    rain_peak: float = 0.0

    def step(self) -> dict:
        self.sim_time += timedelta(minutes=SIMULATION_STEP_MINUTES)

        hour = self.sim_time.hour + self.sim_time.minute / 60.0
        solar = max(0.0, math.sin(math.pi * (hour - 6) / 12.0))

        self.cloud_cover = clamp(self.cloud_cover + random.uniform(-0.08, 0.08), 0.0, 1.0)

        if self.rain_steps_remaining > 0:
            self.rain_steps_remaining -= 1
            fade = max(0.25, self.rain_steps_remaining / max(1, self.rain_steps_remaining + 1))
            base_rain = self.rain_peak * fade
            self.chuva = round(max(0.0, random.gauss(base_rain, 0.35)), 1)
            self.cloud_cover = clamp(self.cloud_cover + random.uniform(0.05, 0.15), 0.5, 1.0)
        else:
            self.chuva = 0.0
            chance_rain = 0.02 + (0.03 if hour >= 15 or hour <= 6 else 0.0) + (0.04 if self.cloud_cover > 0.65 else 0.0)
            if random.random() < chance_rain:
                self.rain_steps_remaining = random.randint(3, 10)
                self.rain_peak = round(random.uniform(1.5, 8.0), 1)
                self.chuva = round(random.uniform(0.8, self.rain_peak), 1)
                self.cloud_cover = clamp(self.cloud_cover + 0.25, 0.6, 1.0)

        target_lux = solar * (1500 * (1.0 - 0.75 * self.cloud_cover))
        target_lux += random.uniform(-40, 40)
        if self.chuva > 0:
            target_lux *= 0.65
        self.luminosidade = clamp(target_lux, 0.0, 1800.0)

        target_air = 19 + (14 * solar) - (4.0 * self.cloud_cover)
        if self.chuva > 0:
            target_air -= min(3.0, self.chuva * 0.35)
        self.temperatura_ar = smooth(self.temperatura_ar, target_air, 0.28, noise=0.35)
        self.temperatura_ar = clamp(self.temperatura_ar, 14.0, 39.0)

        target_humidity = 88 - (30 * solar) + (12 * self.cloud_cover)
        if self.chuva > 0:
            target_humidity += min(12, self.chuva * 2.0)
        self.umidade_ar = smooth(self.umidade_ar, target_humidity, 0.25, noise=1.2)
        self.umidade_ar = clamp(self.umidade_ar, 30.0, 99.0)

        target_wind = 1.5 + (4.5 * solar) + random.uniform(-1.2, 1.2)
        if self.chuva > 0:
            target_wind += random.uniform(4.0, 9.0)
        self.velocidade_vento = smooth(self.velocidade_vento, target_wind, 0.35, noise=0.8)
        self.velocidade_vento = clamp(self.velocidade_vento, 0.0, 28.0)

        evap_factor = (
            max(0.0, solar * 1.6)
            + max(0.0, self.temperatura_ar - 28) * 0.12
            + self.velocidade_vento * 0.03
        )
        soil_delta = -(evap_factor * 0.9)
        if self.chuva > 0:
            soil_delta += self.chuva * random.uniform(0.8, 1.5)
        self.umidade_solo = clamp(self.umidade_solo + soil_delta + random.uniform(-0.5, 0.5), 12.0, 95.0)

        target_soil_temp = 18 + (self.temperatura_ar - 18) * 0.65 + solar * 1.2
        if self.chuva > 0:
            target_soil_temp -= min(1.5, self.chuva * 0.15)
        self.temperatura_solo = smooth(self.temperatura_solo, target_soil_temp, 0.18, noise=0.15)
        self.temperatura_solo = clamp(self.temperatura_solo, 12.0, 34.0)

        ph_drift = random.uniform(-0.03, 0.03)
        if self.chuva > 5:
            ph_drift -= 0.03
        self.ph_solo = clamp(self.ph_solo + ph_drift, 5.1, 7.4)

        if random.random() < 0.03:
            self.umidade_solo = clamp(self.umidade_solo - random.uniform(6, 12), 10.0, 95.0)
        if random.random() < 0.02:
            self.velocidade_vento = clamp(self.velocidade_vento + random.uniform(6, 10), 0.0, 28.0)
        if random.random() < 0.015:
            self.ph_solo = clamp(self.ph_solo + random.choice([-0.4, 0.4]), 5.0, 7.8)

        return {
            "temperatura_ar": round(self.temperatura_ar, 1),
            "temperatura_solo": round(self.temperatura_solo, 1),
            "umidade_ar": int(round(self.umidade_ar)),
            "umidade_solo": int(round(self.umidade_solo)),
            "ph_solo": round(self.ph_solo, 1),
            "luminosidade": int(round(self.luminosidade)),
            "velocidade_vento": round(self.velocidade_vento, 1),
            "chuva": round(self.chuva, 1),
        }


def clamp(value: float, min_value: float, max_value: float) -> float:
    return max(min_value, min(value, max_value))


def smooth(current: float, target: float, alpha: float, noise: float = 0.0) -> float:
    return current + (target - current) * alpha + random.uniform(-noise, noise)


def build_client() -> mqtt.Client:
    client_id = f"agrotech-sim-{socket.gethostname()}-{random.randint(1000, 9999)}"

    client = mqtt.Client(
        callback_api_version=mqtt.CallbackAPIVersion.VERSION2,
        client_id=client_id,
        protocol=mqtt.MQTTv311,
    )

    client.username_pw_set(AIO_USERNAME, AIO_KEY)

    def on_connect(client, userdata, connect_flags, reason_code, properties):
        if reason_code == 0:
            print(f"[OK] Conectado ao Adafruit IO em {MQTT_HOST}:{MQTT_PORT}")
            print(f"[OK] Client ID: {client_id}")
            print(f"[OK] Publicando em: {TOPIC}")
        else:
            print(f"[ERRO] Falha no connect. reason_code={reason_code}")

    def on_disconnect(client, userdata, disconnect_flags, reason_code, properties):
        if reason_code == 0:
            print("[INFO] Desconectado normalmente do broker")
        else:
            print(f"[ERRO] Desconexão inesperada. reason_code={reason_code}")

    def on_publish(client, userdata, mid, reason_code, properties):
        print(f"[PUB] Publish confirmado pelo cliente. mid={mid}, reason_code={reason_code}")

    def on_log(client, userdata, level, buf):
        # Descomente se quiser debug mais verboso do MQTT
        # print(f"[MQTT-LOG] level={level} msg={buf}")
        pass

    client.on_connect = on_connect
    client.on_disconnect = on_disconnect
    client.on_publish = on_publish
    client.on_log = on_log

    return client


def main() -> None:
    if not AIO_USERNAME or not AIO_KEY:
        raise RuntimeError("Defina AIO_USERNAME e AIO_KEY no ambiente.")

    print("[INFO] Simulador iniciando...")
    print(f"[INFO] MQTT host: {MQTT_HOST}:{MQTT_PORT}")
    print(f"[INFO] Feed key: {AIO_FEED_KEY}")
    print(f"[INFO] Tópico final: {TOPIC}")

    client = build_client()

    try:
        client.connect(MQTT_HOST, MQTT_PORT, keepalive=60)
    except Exception as ex:
        print(f"[ERRO] Exceção ao conectar no broker: {ex}")
        raise

    client.loop_start()

    # pequeno tempo para deixar o on_connect aparecer antes do primeiro publish
    time.sleep(2)

    state = FieldState(
        sim_time=datetime.now(timezone.utc),
        temperatura_ar=random.uniform(20.0, 24.0),
        temperatura_solo=random.uniform(19.0, 22.0),
        umidade_ar=random.uniform(60.0, 78.0),
        umidade_solo=random.uniform(45.0, 65.0),
        ph_solo=random.uniform(5.8, 6.5),
        velocidade_vento=random.uniform(1.0, 4.0),
        cloud_cover=random.uniform(0.1, 0.4),
    )

    print("[INFO] Simulador iniciado")
    print(f"[INFO] Intervalo real: {PUBLISH_INTERVAL_SECONDS}s")
    print(f"[INFO] Passo simulado: {SIMULATION_STEP_MINUTES} min")

    try:
        while True:
            payload = state.step()
            payload_json = json.dumps(payload, ensure_ascii=False, separators=(",", ":"))

            info = client.publish(TOPIC, payload_json, qos=0, retain=False)
            info.wait_for_publish(timeout=10)

            print(f"[{state.sim_time.isoformat()}] {payload_json}")

            if info.rc != mqtt.MQTT_ERR_SUCCESS:
                print(f"[ERRO] Falha no publish. rc={info.rc}")
            else:
                print(f"[OK] Publish enviado. mid={info.mid}")

            time.sleep(PUBLISH_INTERVAL_SECONDS)

    except KeyboardInterrupt:
        print("[INFO] Encerrando simulador")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()
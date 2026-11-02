#include <DHTesp.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>

const char* broker = "broker.hivemq.com";

const int port = 1883;
const int DHT_PIN = 15;
const int SOIL_PIN = 34;
const int RELAY_PIN = 26;

unsigned long lastTelemetryTime = 0;
const unsigned long TELEMETRY_INTERVAL = 3000;

int soilMoisture = 0;
bool sensorReady = false;

bool pumpRunning = false;
bool autoMode = true;
unsigned long lastPumpStopTime = 0;
unsigned long pumpStartTime = 0;
const unsigned long PUMP_COOLDOWN = 30000;
const unsigned long PUMP_DURATION = 5000;

unsigned long lastMqttAttemptTime = 0;
const unsigned long MQTT_RECONNECT_INTERVAL = 5000;

DHTesp dht;

const char* telemetryTopic = "iyasense/kode-n-rolla/demo-7f29c1/telemetry";
const char* commandTopic = "iyasense/kode-n-rolla/demo-7f29c1/commands";

WiFiClient wifi;
PubSubClient mqtt(wifi);

void onMessage(char* topic, byte* payload, unsigned int length) {
  String command = "";

  for (unsigned int i = 0; i < length; i++) {
    command += (char)payload[i];
  }

  Serial.print("Received command: ");
  Serial.println(command);

  unsigned long elapsed = millis() - lastPumpStopTime;
  Serial.println(elapsed >= PUMP_COOLDOWN ? 0 : PUMP_COOLDOWN - elapsed);

  if (command == "MODE_MANUAL") {
    autoMode = false;

    if (pumpRunning) {
        stopPump();
    }
  } else if (command == "MODE_AUTO") {
      autoMode = true;

      if (pumpRunning) {
          stopPump();
      }
  } else if (command == "PUMP_START" &&
         !autoMode &&
         sensorReady &&
         !pumpRunning &&
         millis() - lastPumpStopTime >= PUMP_COOLDOWN) {

      startPump();
  } else if (command == "PUMP_STOP" && pumpRunning) {
      stopPump();
  }
}

void connectMqtt() {
  if (!mqtt.connected() &&
      millis() - lastMqttAttemptTime >= MQTT_RECONNECT_INTERVAL) {

    lastMqttAttemptTime = millis();

    if (mqtt.connect("iyasense-esp32-demo-7f29c1")) {
      Serial.println("MQTT connected!");
      mqtt.subscribe(commandTopic);
    } else {
      Serial.println("MQTT connection failed");
    }
  }
}

void startPump() {
    pumpRunning = true;
    pumpStartTime = millis();
    digitalWrite(RELAY_PIN, HIGH);
    Serial.println("Pump ON");
}

void stopPump() {
    pumpRunning = false;
    digitalWrite(RELAY_PIN, LOW);
    lastPumpStopTime = millis();
    Serial.println("Pump OFF");
}

void setup() {
  Serial.begin(115200);
  pinMode(RELAY_PIN, OUTPUT);
  digitalWrite(RELAY_PIN, LOW);

  WiFi.begin("Wokwi-GUEST", "", 6);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("WiFi connected!");

  mqtt.setServer(broker, port);
  mqtt.setCallback(onMessage);

  connectMqtt();
  dht.setup(DHT_PIN, DHTesp::DHT22);
}

void loop() {
  // Stop pump after 5 seconds
  if (pumpRunning && millis() - pumpStartTime >= PUMP_DURATION) {
      stopPump();
  }

  if (!mqtt.connected()) {
    connectMqtt();
  }

  mqtt.loop();

  // Read sensors and publish telemetry every 3 seconds
  if (millis() - lastTelemetryTime >= TELEMETRY_INTERVAL) {
    lastTelemetryTime = millis();

    TempAndHumidity data = dht.getTempAndHumidity();
    int soilRaw = analogRead(SOIL_PIN);
    soilMoisture = map(soilRaw, 0, 4095, 0, 100);
    sensorReady = true;

    unsigned long elapsed = millis() - lastPumpStopTime;

    unsigned long remaining = elapsed >= PUMP_COOLDOWN
        ? 0
        : PUMP_COOLDOWN - elapsed;

    JsonDocument doc;

    doc["temperature"] = data.temperature;
    doc["humidity"] = data.humidity;
    doc["soilMoisture"] = soilMoisture;
    doc["pumpRunning"] = pumpRunning;
    doc["autoMode"] = autoMode;
    doc["cooldownRemaining"] = remaining;

    String payload;
    serializeJson(doc, payload);

    bool published = mqtt.publish(telemetryTopic, payload.c_str());

    Serial.println(payload);

    if (published) {
      Serial.println("Message published!");
    } else {
      Serial.println("Publish failed!");
    }
  }

  if (sensorReady &&
    soilMoisture < 30 &&
    !pumpRunning &&
    autoMode &&
    millis() - lastPumpStopTime >= PUMP_COOLDOWN) {

    startPump();
  }
}

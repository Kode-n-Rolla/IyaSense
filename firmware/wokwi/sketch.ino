#include <DHTesp.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>

const char* broker = "broker.hivemq.com";
const int port = 1883;
const int DHT_PIN = 15;
const int SOIL_PIN = 34;
DHTesp dht;
const char* telemetryTopic = "growcare/my-id-12345/telemetry";
const char* commandTopic = "growcare/my-id-12345/commands";

WiFiClient wifi;
PubSubClient mqtt(wifi);

void onMessage(char* topic, byte* payload, unsigned int length) {
  String command = "";

  for (unsigned int i = 0; i < length; i++) {
    command += (char)payload[i];
  }

  Serial.print("Received command: ");
  Serial.println(command);
}

void connectMqtt() {
  while (!mqtt.connected()) {
    if (mqtt.connect("iyasense-esp32-12345")) {
      Serial.println("MQTT connected!");
      mqtt.subscribe(commandTopic);
    } else {
      Serial.println("MQTT connection failed");
      delay(2000);
    }
  }
}

void setup() {
  Serial.begin(115200);

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
  if (!mqtt.connected()) {
    connectMqtt();
  }

  mqtt.loop();

  // 1. Read data
  TempAndHumidity data = dht.getTempAndHumidity();
  int soilRaw = analogRead(SOIL_PIN);
  int soilMoisture = map(soilRaw, 0, 4095, 0, 100);

  // Serial.print("Soil raw: ");
  // Serial.println(soilRaw);

  // 2. Create JSON
  JsonDocument doc;

  doc["temperature"] = data.temperature;
  doc["humidity"] = data.humidity;
  doc["soilMoisture"] = soilMoisture;

  String payload;
  serializeJson(doc, payload);

  // 3. Send JSON throught MQTT
  bool published = mqtt.publish(telemetryTopic, payload.c_str());

  // 4. Output the result in Serial Monitor
  Serial.println(payload);

  if (published) {
    Serial.println("Message published!");
  } else {
    Serial.println("Publish failed!");
  }

  delay(3000);
}

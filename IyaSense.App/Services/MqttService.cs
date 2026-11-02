using MQTTnet;

namespace IyaSense.App.Services {
    public class MqttService {
        private readonly IMqttClient _client;

        public bool IsConnected => _client.IsConnected;
        public event Action<string>? MessageReceived;
        public event Action? Disconnected;

        public MqttService() {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();

            _client.ApplicationMessageReceivedAsync += OnMessageReceived;
            _client.DisconnectedAsync += OnDisconnected;
        }

        /// <summary>
        /// Establishes a connection to the MQTT broker.
        /// </summary>
        public async Task ConnectAsync() {
            var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("broker.hivemq.com", 1883)
                    .WithClientId($"iyasense-desktop-{Guid.NewGuid():N}")
                    .Build();

            await _client.ConnectAsync(options);
        }

        public async Task SubscribeAsync() {
            var factory = new MqttClientFactory();

            var options = factory
                    .CreateSubscribeOptionsBuilder()
                    .WithTopicFilter("iyasense/kode-n-rolla/demo-7f29c1/telemetry")
                    .Build();

            await _client.SubscribeAsync(options);

            System.Diagnostics.Debug.WriteLine("MQTT subscribed!");
        }

        /// <summary>
        /// Publishes a command to the ESP32 controller.
        /// </summary>
        /// <param name="command">The command payload to send.</param>
        public async Task PublishAsync(string command) {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("iyasense/kode-n-rolla/demo-7f29c1/commands")
                .WithPayload(command)
                .Build();

            await _client.PublishAsync(message);
        }

        private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e) {
            string message = e.ApplicationMessage.ConvertPayloadToString();

            MessageReceived?.Invoke(message);

            System.Diagnostics.Debug.WriteLine($"MQTT received: {message}");

            return Task.CompletedTask;
        }
        private Task OnDisconnected(MqttClientDisconnectedEventArgs e) {
            Disconnected?.Invoke();
            return Task.CompletedTask;
        }

        public async Task DisconnectAsync() {
            if (_client.IsConnected) {
                await _client.DisconnectAsync();
            }
        }
    }
}

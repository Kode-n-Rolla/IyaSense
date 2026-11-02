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
                    .WithTopicFilter("growcare/my-id-12345/telemetry")
                    .Build();

            await _client.SubscribeAsync(options);

            System.Diagnostics.Debug.WriteLine("MQTT subscribed!");
        }

        public async Task PublishAsync(string command) {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("growcare/my-id-12345/commands")
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

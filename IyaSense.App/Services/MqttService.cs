using MQTTnet;

namespace IyaSense.App.Services {
    public class MqttService {
        private readonly IMqttClient _client;

        public bool IsConnected => _client.IsConnected;

        public MqttService() {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();
        }

        public async Task ConnectAsync() {
            var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("broker.hivemq.com", 1883)
                    .WithClientId($"iyasense-desktop-{Guid.NewGuid():N}")
                    .Build();

            await _client.ConnectAsync(options);
        }
    }
}

using IyaSense.App.Commands;
using IyaSense.App.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace IyaSense.App.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        private string _connectionStatusText = "Status: Disconnected";
        private readonly MqttService _mqttService;

        public string ConnectionStatusText {
            get => _connectionStatusText;
            set {
                if (_connectionStatusText == value) {
                    return;
                }

                _connectionStatusText = value;
                OnPropertyChanged(nameof(ConnectionStatusText));
            }
        }

        public ICommand ConnectCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel() {
            ConnectCommand = new AsyncRelayCommand(Connect);
            _mqttService = new MqttService();
            _mqttService.MessageReceived += OnMessageReceived;
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task Connect() {
            ConnectionStatusText = "Status: Connecting...";

            try {
                await _mqttService.ConnectAsync();
                await _mqttService.SubscribeAsync();

                if (_mqttService.IsConnected) {
                    ConnectionStatusText = "Status: Connected";
                } else {
                    ConnectionStatusText = "Status: Disconnected";
                }
            } catch (Exception ex) {
                ConnectionStatusText = "Status: Connection failed";

                System.Diagnostics.Debug.WriteLine($"MQTT connection failed: {ex.Message}");
            }

        }

        private void OnMessageReceived(string message) {
            System.Diagnostics.Debug.WriteLine($"ViewModel received: {message}");
        }
    }
}

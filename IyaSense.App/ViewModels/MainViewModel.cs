using IyaSense.App.Commands;
using IyaSense.App.Models;
using IyaSense.App.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace IyaSense.App.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        private string _connectionStatusText = "Status: Disconnected";
        private string _lastMessage = "No messages received";
        private float _temperature;
        private float _humidity;
        private int _soilMoisture;
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

        public string LastMessage {
            get => _lastMessage;
            set {
                if (_lastMessage == value) {
                    return;
                }

                _lastMessage = value;
                OnPropertyChanged(nameof(LastMessage));
            }
        }

        public float Temperature {
            get => _temperature;
            set {
                if (_temperature == value) {
                    return;
                }

                _temperature = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }


        public float Humidity {
            get => _humidity;
            set {
                if (_humidity == value) {
                    return;
                }

                _humidity = value;
                OnPropertyChanged(nameof(Humidity));
            }
        }

        public int SoilMoisture {
            get => _soilMoisture;
            set {
                if (_soilMoisture == value) {
                    return;
                }

                _soilMoisture = value;
                OnPropertyChanged(nameof(SoilMoisture));
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
            //Application.Current.Dispatcher.BeginInvoke(() => {
            //    LastMessage = message;
            //});
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try {
                var data = JsonSerializer.Deserialize<SensorData>(message, options);

                if (data != null) {
                    Debug.WriteLine($"Temp: {data.Temperature}, Hum: {data.Humidity}");
                    Application.Current.Dispatcher.BeginInvoke(() => {
                        Temperature = data.Temperature;
                        Humidity = data.Humidity;
                        SoilMoisture = data.SoilMoisture;
                    });
                }
            } catch (JsonException ex) {
                Debug.WriteLine($"Invalid JSON: {ex.Message}");
            }

        }
    }
}

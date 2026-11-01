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
        private float _temperature;
        private float _humidity;
        private int _soilMoisture;
        private bool _pumpRunning;
        private bool _autoMode;
        private long _cooldownRemaining;
        private readonly MqttService _mqttService;
        private static readonly JsonSerializerOptions _jsonOptions = new() {
            PropertyNameCaseInsensitive = true
        };

        public string PumpStatusText => PumpRunning ? "Pump: ON" : "Pump: OFF";
        public string ModeStatusText => AutoMode ? "Mode: Automatic" : "Mode: Manual";
        public string ModeToggleButtonText => AutoMode ? "Switch to Manual" : "Switch to Automatic";
        public string PumpToggleButtonText => PumpRunning ? "Stop Pump" : "Start Pump";
        public bool ManualModeEnabled => !AutoMode;
        public string CooldownStatusText => CooldownRemaining > 0 ? $"Cooldown: {(CooldownRemaining + 999) / 1000}s" : "Cooldown: Ready";

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

        public bool PumpRunning {
            get => _pumpRunning;
            set {
                if (_pumpRunning == value) {
                    return;
                }

                _pumpRunning = value;
                OnPropertyChanged(nameof(PumpRunning));
                OnPropertyChanged(nameof(PumpStatusText));
                OnPropertyChanged(nameof(PumpToggleButtonText));
            }
        }

        public bool AutoMode {
            get => _autoMode;
            set {
                if (_autoMode == value) {
                    return;
                }

                _autoMode = value;
                OnPropertyChanged(nameof(AutoMode));
                OnPropertyChanged(nameof(ModeStatusText));
                OnPropertyChanged(nameof(ModeToggleButtonText));
                OnPropertyChanged(nameof(ManualModeEnabled));
            }
        }

        public long CooldownRemaining {
            get => _cooldownRemaining;
            set {
                if (_cooldownRemaining == value) {
                    return;
                }

                _cooldownRemaining = value;
                OnPropertyChanged(nameof(CooldownRemaining));
                OnPropertyChanged(nameof(CooldownStatusText));
            }
        }

        public ICommand ConnectCommand { get; }
        public ICommand ToggleModeCommand { get; }
        public ICommand TogglePumpCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel() {
            ConnectCommand = new AsyncRelayCommand(Connect);
            ToggleModeCommand = new AsyncRelayCommand(ToggleMode);
            TogglePumpCommand = new AsyncRelayCommand(TogglePump);
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
            try {
                var data = JsonSerializer.Deserialize<SensorData>(message, _jsonOptions);

                if (data != null) {
                    Application.Current.Dispatcher.BeginInvoke(() => {
                        Temperature = data.Temperature;
                        Humidity = data.Humidity;
                        SoilMoisture = data.SoilMoisture;
                        PumpRunning = data.PumpRunning;
                        AutoMode = data.AutoMode;
                        CooldownRemaining = data.CooldownRemaining;
                    });
                }
            } catch (JsonException ex) {
                Debug.WriteLine($"Invalid JSON: {ex.Message}");
            }

        }

        private async Task ToggleMode() {
            if (AutoMode) {
                await _mqttService.PublishAsync("MODE_MANUAL");
            } else {
                await _mqttService.PublishAsync("MODE_AUTO");
            }
        }

        private async Task TogglePump() {
            if (PumpRunning) {
                await _mqttService.PublishAsync("PUMP_STOP");
            } else {
                await _mqttService.PublishAsync("PUMP_START");
            }
        }
    }
}

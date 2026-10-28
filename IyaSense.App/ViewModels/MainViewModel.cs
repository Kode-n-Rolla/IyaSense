using IyaSense.App.Commands;
using System.ComponentModel;
using System.Windows.Input;

namespace IyaSense.App.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        private string _connectionStatusText = "Status: Disconnected";

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
            ConnectCommand = new RelayCommand(Connect);
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Connect() {
            ConnectionStatusText = "Status: Connected";
        }
    }
}

using System.ComponentModel;
using System.Windows;

namespace IyaSense.App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private string _connectionStatusText = "Status: Disconnected";

        public string ConnectionStatusText {
            get => _connectionStatusText;
            set {
                _connectionStatusText = value;
                OnPropertyChanged(nameof(ConnectionStatusText));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow() {
            InitializeComponent();

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ConnectionStatusText = "Status: Connected";
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
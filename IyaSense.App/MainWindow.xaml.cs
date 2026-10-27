using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IyaSense.App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private string _connectionStatustext = "Status: Disconnected";

        public string ConnectionStatusText {
            get => _connectionStatustext;
            set {
                _connectionStatustext = value;
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
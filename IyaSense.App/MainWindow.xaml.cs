using System.Windows;
using IyaSense.App.ViewModels;

namespace IyaSense.App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}
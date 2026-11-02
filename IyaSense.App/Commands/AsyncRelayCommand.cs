using System.Windows.Input;

namespace IyaSense.App.Commands {
    public class AsyncRelayCommand : ICommand {
        private readonly Func<Task> _execute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute) {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) {
            return !_isExecuting;
        }

        public async void Execute(object? parameter) {
            try {
                await ExecuteAsync();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Command execution failed: {ex.Message}");
            }
        }

        public async Task ExecuteAsync() {
            if (!CanExecute(null)) {
                return;
            }

            _isExecuting = true;
            OnCanExecuteChanged();

            try {
                await _execute();
            } finally {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        private void OnCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

using System.Windows.Input;

namespace DynamicDataGridSample.Utilities
{
    public class RelayCommand<T>(Action<T> execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<bool>? _canExecute = canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }
    }

    public class RelayCommand(Action execute, Func<bool>? canExecute = null) : RelayCommand<object>(_ => execute(), canExecute)
    {
    }
}

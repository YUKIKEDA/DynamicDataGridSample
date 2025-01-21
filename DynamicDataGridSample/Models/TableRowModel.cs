using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynamicDataGridSample.Models
{
    public class TableRowModel : INotifyPropertyChanged, IDisposable
    {
        private bool _disposed;
        private Dictionary<string, object> _data = [];
        private bool _isSelected = false;

        public Dictionary<string, object> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                PropertyChanged = null;
            }

            _disposed = true;
        }

        ~TableRowModel()
        {
            Dispose(false);
        }
    }
}

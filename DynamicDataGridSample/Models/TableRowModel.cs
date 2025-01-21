using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynamicDataGridSample.Models
{
    public class TableRowModel : INotifyPropertyChanged
    {
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
    }
}

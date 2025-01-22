using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynamicDataGridSample.Models
{
    public abstract class DynamicDataModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object?> _dynamicProperties = [];

        protected T? GetValue<T>(string propertyName)
        {
            if (_dynamicProperties.TryGetValue(propertyName, out var value))
            {
                return value is T typedValue ? typedValue : default;
            }
            return default;
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            if (!_dynamicProperties.TryGetValue(propertyName, out var existingValue) || 
                !EqualityComparer<T>.Default.Equals((T)existingValue!, value))
            {
                _dynamicProperties[propertyName] = value;
                OnPropertyChanged(propertyName);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual IEnumerable<ColumnDefinition> GetColumnDefinitions()
        {
            return Array.Empty<ColumnDefinition>();
        }
    }
} 
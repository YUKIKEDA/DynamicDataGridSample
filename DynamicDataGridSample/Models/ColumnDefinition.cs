using System.Windows.Data;

namespace DynamicDataGridSample.Models
{
    public class ColumnDefinition
    {
        public string Header { get; set; } = string.Empty;
        public string PropertyPath { get; set; } = string.Empty;
        public Type PropertyType { get; set; } = typeof(string);
        public bool IsReadOnly { get; set; } = true;
        public IValueConverter? Converter { get; set; }
        public string? StringFormat { get; set; }
    }
} 
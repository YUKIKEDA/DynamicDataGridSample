using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DynamicDataGridSample.Controls
{
    public class DynamicDataGrid : DataGrid
    {
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.Register(
                nameof(ColumnsSource),
                typeof(ObservableCollection<DataGridColumn>),
                typeof(DynamicDataGrid),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        public ObservableCollection<DataGridColumn> ColumnsSource
        {
            get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsSourceProperty);
            set => SetValue(ColumnsSourceProperty, value);
        }

        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DynamicDataGrid grid)
            {
                grid.Columns.Clear();
                if (e.NewValue is ObservableCollection<DataGridColumn> columns)
                {
                    foreach (var column in columns)
                    {
                        grid.Columns.Add(column);
                    }
                }
            }
        }
    }
} 
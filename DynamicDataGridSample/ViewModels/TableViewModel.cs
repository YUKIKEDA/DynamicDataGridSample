using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DynamicDataGridSample.Models;
using DynamicDataGridSample.Utilities;
using System.Text;

namespace DynamicDataGridSample.ViewModels
{
    public class TableViewModel<T> : INotifyPropertyChanged, IDisposable where T : DynamicDataModel
    {
        private bool _disposed;
        private ObservableCollection<TableRowModel<T>> _rows = [];
        private readonly bool _showCheckBoxColumn;
        private bool? _allSelected = false;
        private int _selectedCount;
        private ObservableCollection<DataGridColumn> _columns = [];

        public bool? AllSelected
        {
            get => _allSelected;
            set
            {
                if (_allSelected != value)
                {
                    _allSelected = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        SelectAll(value.Value);
                    }
                }
            }
        }

        public ObservableCollection<TableRowModel<T>> Rows
        {
            get => _rows;
            set
            {
                // Unsubscribe from the PropertyChanged event of each item in the old collection
                foreach (var item in _rows)
                {
                    item.PropertyChanged -= OnRowPropertyChanged;
                }

                _rows = value;

                // Subscribe to the PropertyChanged event of each item in the new collection
                foreach (var item in _rows)
                {
                    item.PropertyChanged += OnRowPropertyChanged;
                }

                OnPropertyChanged();

                // Update the selected count when the rows collection changes
                UpdateSelectedCount();
            }
        }

        public int SelectedCount
        {
            get => _selectedCount;
            private set
            {
                if (_selectedCount != value)
                {
                    _selectedCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<DataGridColumn> Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<SelectionChangedEventArgs<T>>? SelectionChanged;

        public event EventHandler<ProcessSelectedEventArgs<T>>? ProcessSelected;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ProcessSelectedCommand { get; }

        public TableViewModel(IEnumerable<TableRowModel<T>>? initialData = null, bool showCheckBoxColumn = true)
        {
            _showCheckBoxColumn = showCheckBoxColumn;
            Rows = new ObservableCollection<TableRowModel<T>>(initialData ?? []);
            InitializeColumns();
            ProcessSelectedCommand = new RelayCommand(ProcessSelectedItems, () => SelectedCount > 0);
            UpdateAllSelectedState();
        }

        public string GetCsvData()
        {
            var sb = new StringBuilder();

            // ヘッダー行の作成（データ列のみ）
            var headers = Columns.OfType<DataGridTextColumn>()
                .Select(column => column.Header?.ToString() ?? string.Empty)
                .ToList();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

            // データ行の作成
            foreach (var row in Rows)
            {
                var columnDefinitions = row.Data.GetColumnDefinitions();
                var fields = columnDefinitions
                    .Select(definition => 
                    {
                        var value = row.Data.GetType().GetProperty(definition.PropertyPath)?.GetValue(row.Data)?.ToString() ?? string.Empty;
                        return EscapeCsvField(value);
                    });

                sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString();
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            
            return field;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TableRowModel<T>.IsSelected))
            {
                if (sender is TableRowModel<T> row)
                {
                    UpdateSelectedCount();
                    UpdateAllSelectedState();
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs<T>(row));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = Rows.Count(row => row.IsSelected);
        }

        private void UpdateAllSelectedState()
        {
            if (!Rows.Any())
            {
                AllSelected = false;
                return;
            }

            var selectedCount = Rows.Count(r => r.IsSelected);
            AllSelected = selectedCount == 0 ? false : selectedCount == Rows.Count ? true : null;
        }

        private void SelectAll(bool isSelected)
        {
            foreach (var row in Rows)
            {
                row.IsSelected = isSelected;
            }
        }

        private void ProcessSelectedItems()
        {
            var selectedItems = Rows?.Where(x => x?.IsSelected == true).ToList();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            ProcessSelected?.Invoke(this, new ProcessSelectedEventArgs<T>(selectedItems));
        }

        private void InitializeColumns()
        {
            var columns = new ObservableCollection<DataGridColumn>();

            // 選択列の作成
            if (_showCheckBoxColumn)
            {
                var headerCheckBoxTemplate = CreateHeaderCheckBoxTemplate();
                var cellCheckBoxTemplate = CreateCheckBoxTemplate();

                columns.Add(new DataGridTemplateColumn
                {
                    HeaderTemplate = headerCheckBoxTemplate,
                    CellTemplate = cellCheckBoxTemplate,
                    Width = new DataGridLength(60)
                });
            }

            // データ列の作成
            if (!Rows.Any() || Rows.First().Data == null)
            {
                Columns = columns;
                return;
            }

            var columnDefinitions = Rows.First().Data.GetColumnDefinitions();
            foreach (var definition in columnDefinitions)
            {
                var binding = new Binding($"Data.{definition.PropertyPath}")
                {
                    Converter = definition.Converter,
                    StringFormat = definition.StringFormat
                };

                columns.Add(new DataGridTextColumn
                {
                    Header = definition.Header,
                    Binding = binding,
                    IsReadOnly = definition.IsReadOnly
                });
            }

            Columns = columns;
        }

        private static DataTemplate CreateHeaderCheckBoxTemplate()
        {
            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding("DataContext.AllSelected")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1)
            });
            checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkBoxFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);

            return new DataTemplate { VisualTree = checkBoxFactory };
        }

        private static DataTemplate CreateCheckBoxTemplate()
        {
            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsSelected")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkBoxFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);

            return new DataTemplate { VisualTree = checkBoxFactory };
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
                // イベントハンドラの解除
                foreach (var item in _rows)
                {
                    item.PropertyChanged -= OnRowPropertyChanged;
                }

                // イベントの解除
                PropertyChanged = null;
                SelectionChanged = null;
                ProcessSelected = null;
            }

            _disposed = true;
        }

        ~TableViewModel()
        {
            Dispose(false);
        }
    }

    public class SelectionChangedEventArgs<T>(TableRowModel<T> row) : EventArgs where T : DynamicDataModel
    {
        public TableRowModel<T> Row { get; } = row;
    }

    public class ProcessSelectedEventArgs<T>(IReadOnlyList<TableRowModel<T>> selectedItems) : EventArgs where T : DynamicDataModel
    {
        public IReadOnlyList<TableRowModel<T>> SelectedItems { get; } = selectedItems;
    }
}

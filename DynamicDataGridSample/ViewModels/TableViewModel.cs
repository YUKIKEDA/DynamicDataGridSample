using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DynamicDataGridSample.Models;
using DynamicDataGridSample.Utilities;

namespace DynamicDataGridSample.ViewModels
{
    public class TableViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _disposed;
        private ObservableCollection<TableRowModel> _rows = [];
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

        public ObservableCollection<TableRowModel> Rows
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

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public event EventHandler<ProcessSelectedEventArgs>? ProcessSelected;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ProcessSelectedCommand { get; }

        public TableViewModel(IEnumerable<TableRowModel>? initialData = null, bool showCheckBoxColumn = true)
        {
            _showCheckBoxColumn = showCheckBoxColumn;
            Rows = new ObservableCollection<TableRowModel>(initialData ?? []);
            InitializeColumns();
            ProcessSelectedCommand = new RelayCommand(ProcessSelectedItems, () => SelectedCount > 0);
            UpdateAllSelectedState();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TableRowModel.IsSelected))
            {
                if (sender is TableRowModel row)
                {
                    UpdateSelectedCount();
                    UpdateAllSelectedState();
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(row));
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

            ProcessSelected?.Invoke(this, new ProcessSelectedEventArgs(selectedItems));
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

            foreach (var key in Rows.First().Data.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                columns.Add(new DataGridTextColumn
                {
                    Header = key,
                    Binding = new Binding($"Data[{key}]"),
                    IsReadOnly = true
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

    public class SelectionChangedEventArgs(TableRowModel row) : EventArgs
    {
        public TableRowModel Row { get; } = row;
    }

    public class ProcessSelectedEventArgs(IReadOnlyList<TableRowModel> selectedItems) : EventArgs
    {
        public IReadOnlyList<TableRowModel> SelectedItems { get; } = selectedItems;
    }
}

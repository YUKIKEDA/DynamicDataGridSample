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
    public class TableViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TableRowModel> _rows = [];

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

        private int _selectedCount;

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

        private ObservableCollection<DataGridColumn> _columns = [];
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
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(row));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = Rows.Count(row => row.IsSelected);
        }

        public ICommand SelectAllCommand { get; }

        private void SelectAll(bool isSelected)
        {
            foreach (var row in Rows)
            {
                row.IsSelected = isSelected;
            }
        }

        public ICommand ProcessSelectedCommand { get; }

        private void ProcessSelectedItems()
        {
            var selectedItems = Rows?.Where(x => x?.IsSelected == true).ToList();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            ProcessSelected?.Invoke(this, new ProcessSelectedEventArgs(selectedItems));
        }

        private readonly bool _showCheckBoxColumn;

        public TableViewModel(
            IEnumerable<TableRowModel>? initialData = null,
            bool showCheckBoxColumn = true)
        {
            _showCheckBoxColumn = showCheckBoxColumn;
            Rows = new ObservableCollection<TableRowModel>(initialData ?? []);
            InitializeColumns();

            SelectAllCommand = new RelayCommand<bool>(SelectAll);
            ProcessSelectedCommand = new RelayCommand(ProcessSelectedItems, () => SelectedCount > 0);
        }

        private void InitializeColumns()
        {
            var columns = new ObservableCollection<DataGridColumn>();

            // 選択列の作成
            if (_showCheckBoxColumn)
            {
                var checkBoxTemplate = CreateCheckBoxTemplate();
                columns.Add(new DataGridTemplateColumn
                {
                    Header = "選択",
                    CellTemplate = checkBoxTemplate,
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

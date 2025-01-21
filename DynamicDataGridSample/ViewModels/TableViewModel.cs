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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TableRowModel.IsSelected))
            {
                var row = (TableRowModel)sender!;

                UpdateSelectedCount();

                // Raise the SelectionChanged event
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(row));

                // Invalidate the RequerySuggested event
                CommandManager.InvalidateRequerySuggested();
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

        private bool CanProcessSelected()
        {
            return SelectedCount > 0;
        }

        private void ProcessSelected()
        {
            var selectedItems = Rows.Where(x => x.IsSelected).ToList();
            // 選択されたアイテムの処理をここに実装
        }


        public TableViewModel()
        {
            InitializeData();
            InitializeColumns();

            SelectAllCommand = new RelayCommand<bool>(SelectAll);
            ProcessSelectedCommand = new RelayCommand(ProcessSelected, CanProcessSelected);
        }

        private void InitializeData()
        {
            var items = new ObservableCollection<TableRowModel>();

            var item1 = new TableRowModel
            {
                IsSelected = false,
                Data = new Dictionary<string, object>
            {
                { "ID", 1 },
                { "Name", "Sample 1" },
                { "Value", 100 }
            }
            };

            var item2 = new TableRowModel
            {
                IsSelected = false,
                Data = new Dictionary<string, object>
            {
                { "ID", 2 },
                { "Name", "Sample 2" },
                { "Value", 200 }
            }
            };

            items.Add(item1);
            items.Add(item2);

            // Items プロパティを通して設定することで、イベントハンドラが適切に設定される
            Rows = items;
        }

        private void InitializeColumns()
        {
            Columns = new ObservableCollection<DataGridColumn>();

            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsSelected") 
            { 
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged 
            });
            checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkBoxFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);

            var checkBoxTemplate = new DataTemplate { VisualTree = checkBoxFactory };

            Columns.Add(new DataGridTemplateColumn
            {
                Header = "選択",
                CellTemplate = checkBoxTemplate,
                Width = new DataGridLength(60)
            });

            if (Rows.Any())
            {
                var firstItem = Rows.First().Data;
                foreach (var key in firstItem.Keys)
                {
                    Columns.Add(new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"Data[{key}]"),
                        IsReadOnly = true
                    });
                }
            }
        }
    }

    public class SelectionChangedEventArgs(TableRowModel row) : EventArgs
    {
        public TableRowModel Row { get; } = row;
    }
}

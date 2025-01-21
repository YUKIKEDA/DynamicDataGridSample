using System.Collections.Generic;
using System.Windows;
using DynamicDataGridSample.Models;
using DynamicDataGridSample.ViewModels;

namespace DynamicDataGridSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TableViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var initialData = new List<TableRowModel>
            {
                new()
                {
                    Data = new Dictionary<string, object>
                    {
                        { "ID", 1 },
                        { "Name", "Sample 1" },
                        { "Value", 100 }
                    }
                },
                new()
                {
                    Data = new Dictionary<string, object>
                    {
                        { "ID", 2 },
                        { "Name", "Sample 2" },
                        { "Value", 200 }
                    }
                }
            };

            _viewModel = new TableViewModel(initialData);
            _viewModel.ProcessSelected += OnProcessSelected;
            _viewModel.ProcessSelected += LogSelectedItems;

            // _viewModel = new TableViewModel();

            DataContext = _viewModel;
        }

        private void OnProcessSelected(object? sender, ProcessSelectedEventArgs e)
        {
            MessageBox.Show($"選択されたアイテム数: {e.SelectedItems.Count}", "選択アイテム");
        }

        private void LogSelectedItems(object? sender, ProcessSelectedEventArgs e)
        {
            // ログ出力の例
            System.Diagnostics.Debug.WriteLine($"処理されたアイテム数: {e.SelectedItems.Count}");
            foreach (var item in e.SelectedItems)
            {
                System.Diagnostics.Debug.WriteLine($"ID: {item.Data["ID"]}, Name: {item.Data["Name"]}");
            }
        }
    }
}
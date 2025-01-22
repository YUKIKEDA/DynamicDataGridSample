using System.Collections.Generic;
using System.Windows;
using DynamicDataGridSample.Models;
using DynamicDataGridSample.ViewModels;

namespace DynamicDataGridSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private bool _disposed;
        private readonly TableViewModel<SampleDataModel> _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var initialData = new List<TableRowModel<SampleDataModel>>
            {
                new(new SampleDataModel
                {
                    Id = 1,
                    Name = "Sample 1",
                    Value = 100,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsActive = true
                }),
                new(new SampleDataModel
                {
                    Id = 2,
                    Name = "Sample 2",
                    Value = 200,
                    CreatedAt = DateTime.Now,
                    IsActive = false
                })
            };

            _viewModel = new TableViewModel<SampleDataModel>(initialData);
            _viewModel.ProcessSelected += OnProcessSelected;
            _viewModel.ProcessSelected += LogSelectedItems;
            System.Diagnostics.Debug.WriteLine(_viewModel.GetCsvData());
            // _viewModel = new TableViewModel();

            DataContext = _viewModel;
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
                _viewModel.ProcessSelected -= OnProcessSelected;
                _viewModel.ProcessSelected -= LogSelectedItems;
                _viewModel.Dispose();
            }

            _disposed = true;
        }

        ~MainWindow()
        {
            Dispose(false);
        }

        private void OnProcessSelected(object? sender, ProcessSelectedEventArgs<SampleDataModel> e)
        {
            MessageBox.Show($"選択されたアイテム数: {e.SelectedItems.Count}", "選択アイテム");
        }

        private void LogSelectedItems(object? sender, ProcessSelectedEventArgs<SampleDataModel> e)
        {
            // ログ出力の例
            System.Diagnostics.Debug.WriteLine($"処理されたアイテム数: {e.SelectedItems.Count}");
            foreach (var item in e.SelectedItems)
            {
                System.Diagnostics.Debug.WriteLine($"ID: {item.Data.Id}, Name: {item.Data.Name}");
            }
        }
    }
}
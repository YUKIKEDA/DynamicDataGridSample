using System.Windows;
using DynamicDataGridSample.ViewModels;

namespace DynamicDataGridSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TableViewModel();
        }
    }
}
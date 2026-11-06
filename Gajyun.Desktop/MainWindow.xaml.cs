using System.Windows;
using Gajyun.Desktop.ViewModels;

namespace Gajyun.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

using System.Windows;
using Microsoft.Win32;

namespace Gajyun.Desktop.Services;

public interface IFileDialogService
{
    string? SelectCsvFile();
}

public sealed class FileDialogService : IFileDialogService
{
    public string? SelectCsvFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a reservation CSV for preview",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
}

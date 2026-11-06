using System.Data;
using System.Windows.Controls;

namespace Gajyun.Desktop.Views;

public partial class CsvImportView : UserControl
{
    public CsvImportView() => InitializeComponent();

    private void PreviewAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (sender is DataGrid { ItemsSource: DataView view } && view.Table?.Columns[e.PropertyName] is DataColumn column)
            e.Column.Header = column.Caption;
    }
}

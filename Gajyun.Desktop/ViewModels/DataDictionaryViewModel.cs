using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Gajyun.Core.Dictionary;
using Microsoft.Extensions.Logging;

namespace Gajyun.Desktop.ViewModels;

public partial class DataDictionaryViewModel : ObservableObject
{
    private readonly DictionaryCatalog? catalog;
    public ObservableCollection<DictionaryTable> Tables { get; } = [];
    public ObservableCollection<DictionaryField> Fields { get; } = [];
    public string SchemaVersion { get; }
    public string Status { get; }

    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private DictionaryTable? selectedTable;
    [ObservableProperty] private DictionaryField? selectedField;

    public DataDictionaryViewModel(IDataDictionaryService service, ILogger<DataDictionaryViewModel> logger)
    {
        try
        {
            catalog = service.Load();
            SchemaVersion = $"Schema v{catalog.SchemaVersion} - implemented model";
            Status = $"{catalog.Tables.Count(x => !x.IsView)} tables and {catalog.Tables.Count(x => x.IsView)} financial view. Available without a database connection. Business descriptions translated from the supplied v1.1 dictionary.";
            ApplySearch();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "The embedded schema dictionary could not be loaded.");
            SchemaVersion = "Dictionary unavailable";
            Status = exception.Message;
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogError(exception, "The embedded dictionary metadata is invalid.");
            SchemaVersion = "Dictionary unavailable";
            Status = "The embedded dictionary metadata is invalid. No database connection is required, but the application metadata must be repaired.";
        }
    }

    partial void OnSearchTextChanged(string value) => ApplySearch();
    partial void OnSelectedTableChanged(DictionaryTable? value) => UpdateFields();

    private void ApplySearch()
    {
        var previousName = SelectedTable?.Name;
        var term = SearchText.Trim();
        Tables.Clear();
        if (catalog is null) return;
        foreach (var table in catalog.Tables.Where(x => Matches(x.Name, term) || x.Fields.Any(field => Matches(field.Name, term))))
            Tables.Add(table);
        SelectedTable = Tables.FirstOrDefault(x => x.Name == previousName) ?? Tables.FirstOrDefault();
        UpdateFields();
    }

    private void UpdateFields()
    {
        Fields.Clear();
        SelectedField = null;
        if (SelectedTable is null) return;
        var term = SearchText.Trim();
        foreach (var field in SelectedTable.Fields.Where(x => Matches(SelectedTable.Name, term) || Matches(x.Name, term)))
            Fields.Add(field);
        SelectedField = Fields.FirstOrDefault();
    }

    private static bool Matches(string value, string term) => value.Contains(term, StringComparison.OrdinalIgnoreCase);
}

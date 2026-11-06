using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gajyun.Core.Services;
using Microsoft.Extensions.Logging;

namespace Gajyun.Desktop.ViewModels;

public sealed record NavigationItem(string Title, ObservableObject Page);

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDatabaseInitializationService databaseInitialization;
    private readonly ILogger<MainWindowViewModel> logger;
    public CsvImportViewModel CsvImport { get; }
    public SettingsViewModel Settings { get; }
    public IReadOnlyList<NavigationItem> NavigationItems { get; }

    [ObservableProperty]
    private NavigationItem selectedNavigation;

    public MainWindowViewModel(IDatabaseInitializationService databaseInitialization, ILogger<MainWindowViewModel> logger,
        CsvImportViewModel csvImport, DataDictionaryViewModel dictionary, SettingsViewModel settings)
    {
        this.databaseInitialization = databaseInitialization;
        this.logger = logger;
        CsvImport = csvImport;
        Settings = settings;
        NavigationItems = [new("CSV Import", csvImport), new("Data Dictionary", dictionary), new("Settings", settings)];
        selectedNavigation = NavigationItems[0];
    }

    public void CancelOperations()
    {
        InitializeCommand.Cancel();
        CsvImport.SelectFileCommand.Cancel();
        CsvImport.ReloadCommand.Cancel();
        CsvImport.RefreshOtasCommand.Cancel();
        CsvImport.ValidateFileCommand.Cancel();
        CsvImport.ImportCommand.Cancel();
        Settings.LoadCommand.Cancel();
        Settings.SavePolicyCommand.Cancel();
        foreach (var room in Settings.RoomPolicies) room.SaveCommand.Cancel();
        Settings.LoadAliasesCommand.Cancel();
    }

    [ObservableProperty]
    private string databaseStatus = "Database initialization has not started.";

    [ObservableProperty]
    private string serverStatus = "Not checked";

    [ObservableProperty]
    private bool isInitializing;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IsInitializing = true;
        ServerStatus = "Checking SQL Server...";
        DatabaseStatus = "Checking connectivity, applying versioned migrations and seeding required data...";
        try
        {
            var result = await databaseInitialization.InitializeAsync(cancellationToken);
            ServerStatus = result.ServerConnected ? "Connected" : "Unavailable";
            DatabaseStatus = result.Message;
            logger.LogInformation("Database startup completed. Ready: {Ready}; server connected: {Connected}.",
                result.Succeeded, result.ServerConnected);
            if (result.Succeeded)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Settings.LoadCommand.ExecuteAsync(null);
                cancellationToken.ThrowIfCancellationRequested();
                await CsvImport.RefreshOtasCommand.ExecuteAsync(null);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ServerStatus = "Check cancelled";
            DatabaseStatus = "Initialization cancelled. Completed migrations may remain; restarting will check the current state safely.";
            logger.LogInformation("Database initialization cancelled.");
        }
        finally
        {
            IsInitializing = false;
        }
    }
}

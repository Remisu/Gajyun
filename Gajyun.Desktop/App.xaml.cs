using System.IO;
using System.Text.Json;
using System.Windows;
using Gajyun.Core.Csv;
using Gajyun.Core.Services;
using Gajyun.Data;
using Gajyun.Desktop.Services;
using Gajyun.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gajyun.Desktop;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                Args = e.Args,
                ContentRootPath = AppContext.BaseDirectory
            });
            var localSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Gajyun", "appsettings.json");
            builder.Configuration.AddJsonFile(localSettings, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("GAJYUN_");
            builder.Logging.AddDebug();
            builder.Services.AddGajyunData(builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing from appsettings.json."));
            builder.Services.AddSingleton<IFileDialogService, FileDialogService>();
            builder.Services.AddSingleton<CsvImportViewModel>();
            builder.Services.AddSingleton<DataDictionaryViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<MainWindowViewModel>();
            builder.Services.AddSingleton<MainWindow>();
            host = builder.Build();
            await host.StartAsync();
            MainWindow = host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
            await host.Services.GetRequiredService<MainWindowViewModel>().InitializeCommand.ExecuteAsync(null);
        }
        catch (IOException exception)
        {
            ShowStartupFailure("Application configuration could not be read. " + exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            ShowStartupFailure("Access to local application configuration was denied. " + exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            ShowStartupFailure("Application startup could not complete. " + exception.Message);
        }
        catch (ArgumentException exception)
        {
            ShowStartupFailure("Application configuration contains an invalid setting. " + exception.Message);
        }
        catch (JsonException exception)
        {
            ShowStartupFailure("Application configuration contains invalid JSON. " + exception.Message);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            host.Services.GetRequiredService<MainWindowViewModel>().CancelOperations();
            try
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException)
            {
                host.Services.GetRequiredService<ILogger<App>>().LogWarning("Application host shutdown timed out.");
            }
            finally
            {
                host.Dispose();
            }
        }
        base.OnExit(e);
    }

    private void ShowStartupFailure(string message)
    {
        host?.Services.GetRequiredService<ILogger<App>>().LogError("Startup failed: {Message}", message);
        MessageBox.Show(message, "Gajyun startup", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown(1);
    }
}

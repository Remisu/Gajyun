using Gajyun.Core.Csv;
using Gajyun.Core.Dictionary;
using Gajyun.Core.Services;
using Gajyun.Data.Metadata;
using Gajyun.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gajyun.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddGajyunData(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContextFactory<GajyunDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton<RoomSeedCatalog>();
        services.AddTransient<DatabaseSeeder>();
        services.AddTransient<IDatabaseInitializationService, DatabaseInitializationService>();
        services.AddSingleton<IDataDictionaryService, DataDictionaryService>();
        services.AddTransient<IRoomSettingsService, RoomSettingsService>();
        services.AddSingleton<CsvPreviewService>();
        services.AddSingleton<ICsvPreviewService>(provider => provider.GetRequiredService<CsvPreviewService>());
        services.AddSingleton<ICsvImportFileReader>(provider => provider.GetRequiredService<CsvPreviewService>());
        services.AddSingleton<NightlyDetailsParser>();
        services.AddSingleton<IReservationValidationService, ReservationValidationService>();
        services.AddSingleton<ICsvReservationParser, CombinedExportReservationParser>();
        services.AddSingleton<IRoomMappingService, RoomMappingService>();
        services.AddSingleton<IReservationImportService, ReservationImportService>();
        return services;
    }
}

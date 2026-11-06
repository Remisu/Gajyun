using System.Data;
using System.Text.Json;
using Gajyun.Core.Services;
using Gajyun.Data.Seeding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gajyun.Data;

public sealed class DatabaseInitializationService(
    IDbContextFactory<GajyunDbContext> contextFactory,
    DatabaseSeeder seeder,
    ILogger<DatabaseInitializationService> logger) : IDatabaseInitializationService
{
    public async Task<DatabaseInitializationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        var serverConnected = false;
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var connectionString = db.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The SQL Server connection string is not configured.");
            var target = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(target.InitialCatalog) ||
                new[] { "master", "model", "msdb", "tempdb" }.Contains(target.InitialCatalog, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("A non-system application database must be configured.");

            var masterString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master", ConnectTimeout = 5 };
            await using var master = new SqlConnection(masterString.ConnectionString);
            await master.OpenAsync(cancellationToken);
            serverConnected = true;
            logger.LogInformation("SQL Server connectivity verified.");

            await using var exists = master.CreateCommand();
            exists.CommandText = "SELECT CASE WHEN DB_ID(@database) IS NULL THEN 0 ELSE 1 END";
            exists.Parameters.Add(new SqlParameter("@database", SqlDbType.NVarChar, 128) { Value = target.InitialCatalog });
            var databaseExists = Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) == 1;
            if (databaseExists)
                await RejectUnmanagedDatabaseAsync(db, cancellationToken);
            else
                logger.LogInformation("Database {DatabaseName} does not exist. EF Core migrations will create it.", target.InitialCatalog);

            var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            logger.LogInformation("Found {MigrationCount} pending database migration(s).", pending.Length);
            await db.Database.MigrateAsync(cancellationToken);
            await seeder.SeedAsync(db, cancellationToken);
            var databaseStatus = databaseExists ? "Database ready." : "Database created and ready.";
            return new DatabaseInitializationResult(true, true,
                $"{databaseStatus} Applied {pending.Length} migration(s); room, bed, alias and lookup seeding completed.");
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "SQL Server initialization failed; the database was not deleted or recreated.");
            return new DatabaseInitializationResult(false, serverConnected,
                $"Database initialization failed (SQL error {exception.Number}). Check SQL Server, Windows permissions (including database creation) and the connection settings. Existing data was not deleted.");
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Required seed data could not be saved; the seed transaction was rolled back.");
            return new DatabaseInitializationResult(false, serverConnected,
                "Required seed data could not be saved. Check existing room and lookup records for conflicts. No seed changes were committed; completed schema migrations remain applied.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Database initialization could not continue safely.");
            return new DatabaseInitializationResult(false, serverConnected, exception.Message);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "The embedded seed data is invalid.");
            return new DatabaseInitializationResult(false, serverConnected, "The embedded room seed is invalid. No seed changes were committed.");
        }
    }

    private static async Task RejectUnmanagedDatabaseAsync(GajyunDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT COUNT(*) FROM sys.tables
                WHERE is_ms_shipped = 0 AND name <> N'__EFMigrationsHistory';
                """;
            var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            if (tableCount > 0 && !(await db.Database.GetAppliedMigrationsAsync(cancellationToken)).Any())
                throw new InvalidOperationException("This database already contains tables without an EF migration history. Automatic initialization was stopped. Back up the database and arrange an explicit baseline migration; no tables were deleted.");
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}

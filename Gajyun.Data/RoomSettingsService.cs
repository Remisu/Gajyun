using Gajyun.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gajyun.Data;

public sealed class RoomSettingsService(IDbContextFactory<GajyunDbContext> contextFactory, ILogger<RoomSettingsService> logger) : IRoomSettingsService
{
    public Task<RoomSettingsSnapshot> LoadAsync(CancellationToken cancellationToken = default) =>
        WithDatabaseAsync(async db =>
        {
            var rooms = await db.Rooms.AsNoTracking().OrderBy(x => x.RoomID)
                .Select(x => new RoomSettings(x.RoomID, x.RoomName, x.Capacity, x.DefaultTaxPolicyID, x.IsActive))
                .ToArrayAsync(cancellationToken);
            var policies = await db.TaxPolicies.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ValidFrom)
                .Select(x => new TaxPolicyOption(x.TaxPolicyID, x.PolicyName)).ToArrayAsync(cancellationToken);
            var otas = await db.OTAs.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.OTAName)
                .Select(x => new OtaOption(x.OTAID, x.OTAName)).ToArrayAsync(cancellationToken);
            return new RoomSettingsSnapshot(rooms, policies, otas);
        }, cancellationToken);

    public Task<IReadOnlyList<RoomAliasSettings>> GetAliasesAsync(int roomID, CancellationToken cancellationToken = default) =>
        WithDatabaseAsync<IReadOnlyList<RoomAliasSettings>>(async db => await db.OTARoomMappings.AsNoTracking()
            .Where(x => x.RoomID == roomID).OrderBy(x => x.OTAID).ThenBy(x => x.ExternalRoomName)
            .Select(x => new RoomAliasSettings(x.OTAID, x.ExternalRoomName, x.NormalizedName, x.IsActive))
            .ToArrayAsync(cancellationToken), cancellationToken);

    public async Task SetTaxPolicyAsync(int roomID, int? policyID, int? originalPolicyID, CancellationToken cancellationToken = default)
    {
        await WithDatabaseAsync(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            if (policyID.HasValue && !await db.TaxPolicies.AnyAsync(x => x.TaxPolicyID == policyID && x.IsActive, cancellationToken))
                throw new SettingsOperationException("The selected tax policy is no longer active. Refresh Settings and choose again.");
            var affected = await db.Rooms.Where(x => x.RoomID == roomID && x.DefaultTaxPolicyID == originalPolicyID)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DefaultTaxPolicyID, policyID), cancellationToken);
            if (affected != 1)
                throw new SettingsOperationException("The room or its policy changed since loading. Refresh Settings before saving.");
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Saved internal tax-policy preference for room {RoomID}. Historical tax snapshots were not modified.", roomID);
            return true;
        }, cancellationToken);
    }

    private async Task<T> WithDatabaseAsync<T>(Func<GajyunDbContext, Task<T>> operation, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await operation(db);
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Room settings database operation failed.");
            throw new SettingsOperationException($"Room settings could not access SQL Server (error {exception.Number}). Check database status and retry.", exception);
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Room settings could not be saved.");
            throw new SettingsOperationException("The room policy could not be saved because of a database constraint. Refresh and retry.", exception);
        }
    }
}

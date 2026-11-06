using System.Data;
using Gajyun.Core.Models;
using Gajyun.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gajyun.Data.Seeding;

public sealed class DatabaseSeeder(RoomSeedCatalog catalog, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(GajyunDbContext db, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = N'Gajyun.Seed.v1.1',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
            IF @result < 0 THROW 51000, 'Could not acquire the database seed lock.', 1;
            """, cancellationToken);

        foreach (var seed in catalog.Rooms)
        {
            var room = await db.Rooms.SingleOrDefaultAsync(x => x.RoomName == seed.Name, cancellationToken);
            if (room is null)
            {
                room = new Room { RoomName = seed.Name, Capacity = seed.Capacity };
                db.Rooms.Add(room);
                await db.SaveChangesAsync(cancellationToken);
            }
            else if (room.Capacity != seed.Capacity)
            {
                logger.LogWarning("Room {RoomID} has capacity {ActualCapacity}, but the seed expects {ExpectedCapacity}. Existing data was preserved.",
                    room.RoomID, room.Capacity, seed.Capacity);
            }

            foreach (var label in seed.Beds)
            {
                if (!await db.Beds.AnyAsync(x => x.RoomID == room.RoomID && x.BedLabel == label, cancellationToken))
                    db.Beds.Add(new Bed { RoomID = room.RoomID, BedLabel = label });
            }

            foreach (var alias in seed.Aliases.Prepend(seed.Name).Distinct(StringComparer.Ordinal))
            {
                var normalized = RoomNameNormalizer.Normalize(alias);
                if (!await db.OTARoomMappings.AnyAsync(x => x.OTAID == null && x.RoomID == room.RoomID && x.NormalizedName == normalized, cancellationToken) &&
                    !db.OTARoomMappings.Local.Any(x => x.OTAID == null && x.RoomID == room.RoomID && x.NormalizedName == normalized))
                {
                    db.OTARoomMappings.Add(new OTARoomMapping
                    {
                        RoomID = room.RoomID,
                        ExternalRoomName = alias,
                        NormalizedName = normalized
                    });
                }
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        foreach (var name in new[] { "Confirmed", "Cancelled", "Pending" })
            if (!await db.Set<BookingStatus>().AnyAsync(x => x.StatusName == name, cancellationToken))
                db.Add(new BookingStatus { StatusName = name });
        foreach (var name in new[] { "Cash", "Credit Card", "Bank Transfer", "Other" })
            if (!await db.Set<PaymentMethod>().AnyAsync(x => x.MethodName == name, cancellationToken))
                db.Add(new PaymentMethod { MethodName = name });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Room, bed, alias and lookup seeding completed. Existing settings and historical records were preserved.");
    }


}

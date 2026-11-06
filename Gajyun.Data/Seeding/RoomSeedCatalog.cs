using System.Text.Json;
using System.Text.Json.Serialization;
using Gajyun.Core.Services;

namespace Gajyun.Data.Seeding;

public sealed record RoomSeed(
    string Name, short Capacity, string RoomType, string[] Aliases, string[] Beds);

public sealed class RoomSeedCatalog
{
    public IReadOnlyList<RoomSeed> Rooms { get; }

    public RoomSeedCatalog()
    {
        using var stream = typeof(RoomSeedCatalog).Assembly.GetManifestResourceStream("Gajyun.RoomSeed.json")
            ?? throw new InvalidOperationException("The embedded room seed file is missing.");
        var document = JsonSerializer.Deserialize<SeedDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectRequiredConstructorParameters = true,
            RespectNullableAnnotations = true
        }) ?? throw new InvalidOperationException("The room seed file is empty.");
        var rooms = document.Rooms;
        if (rooms is null || rooms.Length != 5 || rooms.Any(x => x is null) || rooms.Sum(x => x.Capacity) != 18 ||
            rooms.Sum(x => x.Beds?.Length ?? 0) != 14 ||
            rooms.Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != 5)
            throw new InvalidOperationException("The room seed must define five unique rooms, capacity 18 and 14 beds.");

        foreach (var room in rooms)
        {
            if (string.IsNullOrWhiteSpace(room.Name) || room.Name.Length > 100 || room.Capacity <= 0 ||
                room.Aliases is null || room.Beds is null ||
                room.Aliases.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 255 || RoomNameNormalizer.Normalize(x).Length > 255) ||
                room.Beds.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 80) ||
                room.Beds.Distinct(StringComparer.Ordinal).Count() != room.Beds.Length)
                throw new InvalidOperationException("The room seed contains an invalid room, alias or duplicate bed position.");
        }
        Rooms = Array.AsReadOnly(rooms);
    }

    private sealed record SeedDocument(RoomSeed[] Rooms);
}

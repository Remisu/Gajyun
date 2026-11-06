using Gajyun.Core.Models;

namespace Gajyun.Core.Services;

public enum RoomMatchStatus { Matched, Unmapped, Ambiguous }
public sealed record RoomMatchResult(RoomMatchStatus Status, int? RoomID, string Message);

public interface IRoomMappingService
{
    RoomMatchResult Match(int? otaID, string externalName, IReadOnlyCollection<OTARoomMapping> mappings);
}

public sealed class RoomMappingService : IRoomMappingService
{
    public RoomMatchResult Match(int? otaID, string externalName, IReadOnlyCollection<OTARoomMapping> mappings)
    {
        ArgumentNullException.ThrowIfNull(externalName);
        ArgumentNullException.ThrowIfNull(mappings);
        var active = mappings.Where(x => x.IsActive).ToArray();
        var normalized = RoomNameNormalizer.Normalize(externalName);
        if (normalized.Length == 0) return new(RoomMatchStatus.Unmapped, null, "The source room name is empty.");
        if (otaID.HasValue)
        {
            var specific = active.Where(x => x.OTAID == otaID && x.NormalizedName == normalized).ToArray();
            if (specific.Length > 0) return Resolve(specific, "OTA room name");
        }
        var generic = active.Where(x => x.OTAID is null && x.NormalizedName == normalized).ToArray();
        return generic.Length == 0
            ? new(RoomMatchStatus.Unmapped, null, "No known room alias matches. Staff mapping is required; no room will be created.")
            : Resolve(generic, "generic room alias");
    }

    private static RoomMatchResult Resolve(IEnumerable<OTARoomMapping> mappings, string source)
    {
        var rooms = mappings.Select(x => x.RoomID).Distinct().ToArray();
        return rooms.Length == 1
            ? new(RoomMatchStatus.Matched, rooms[0], $"Matched by {source}.")
            : new(RoomMatchStatus.Ambiguous, null, $"The {source} points to multiple rooms. Staff confirmation is required.");
    }
}

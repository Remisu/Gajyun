namespace Gajyun.Core.Services;

public sealed record TaxPolicyOption(int? TaxPolicyID, string PolicyName);
public sealed record OtaOption(int? OTAID, string OTAName);
public sealed record RoomSettings(int RoomID, string RoomName, short Capacity, int? DefaultTaxPolicyID, bool IsActive);
public sealed record RoomAliasSettings(int? OTAID, string ExternalRoomName, string NormalizedName, bool IsActive);
public sealed record RoomSettingsSnapshot(IReadOnlyList<RoomSettings> Rooms, IReadOnlyList<TaxPolicyOption> Policies,
    IReadOnlyList<OtaOption> OTAs);

public sealed class SettingsOperationException(string message, Exception? innerException = null) : Exception(message, innerException);

public interface IRoomSettingsService
{
    Task<RoomSettingsSnapshot> LoadAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomAliasSettings>> GetAliasesAsync(int roomID, CancellationToken cancellationToken = default);
    Task SetTaxPolicyAsync(int roomID, int? policyID, int? originalPolicyID, CancellationToken cancellationToken = default);
}

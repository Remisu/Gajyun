namespace Gajyun.Core.Csv;

public sealed record CsvImportFile(byte[] OriginalBytes, string FileName, CsvPreviewResult Content);

public interface ICsvImportFileReader
{
    Task<CsvImportFile> ReadForImportAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default);
}

public sealed record NightlyRate(DateOnly ServiceDate, string Description, decimal RoomPrice, int? ReportedGuestCount);
public sealed record SourceStatement(string Label, string RawText, decimal Amount);
public sealed record NightlyDetails(IReadOnlyList<NightlyRate> Nights, IReadOnlyList<SourceStatement> Statements);

public sealed record ParsedReservation(
    string OTAName, string ExternalNumber, string GuestName, string ExternalRoomName, string RoomMaster,
    DateOnly CheckIn, DateOnly CheckOut, short GuestCount, bool IsCancelled,
    bool IsModified, DateTime BookedAt, DateTime? ModifiedAtSource, string Details, string ConsecutiveInfo,
    string RoomRateInfo, IReadOnlyList<NightlyRate> Nights, IReadOnlyList<SourceStatement> Statements)
{
    public string? SourcePaymentMethod { get; init; }
}

public sealed record ReservationParseResult(ParsedReservation? Reservation, string Message)
{
    public bool Succeeded => Reservation is not null;
}

public enum ImportRowDisposition { Ready, Cancelled, Duplicate, Held, Invalid, Changed, CancellationUpdate }
public sealed record ImportRoomOption(int RoomID, string RoomName);
public sealed record ImportRowDecision(int RowNumber, int? RoomID = null);
public sealed record ReservationComparisonField(string Label, string ExistingValue, string IncomingValue)
{
    public bool IsDifferent => ExistingValue != IncomingValue;
}
public sealed record ImportReviewRow(int RowNumber, string OTAName, ImportRowDisposition Disposition,
    string Message, int NightCount, decimal AccommodationTotal)
{
    public string ReservationNumber { get; init; } = "";
    public string GuestName { get; init; } = "";
    public DateTime? BookedAt { get; init; }
    public DateOnly? CheckIn { get; init; }
    public DateOnly? CheckOut { get; init; }
    public string RoomName { get; init; } = "";
    public bool HasExistingReservation { get; init; }
    public IReadOnlyList<ReservationComparisonField> ComparisonFields { get; init; } = [];
    public IReadOnlyList<ImportRoomOption> RoomChoices { get; init; } = [];
}
public sealed record ReservationImportReview(Guid ReviewID, int TotalRows, int ReadyRows, int CancelledRows,
    int DuplicateRows, int HeldRows, int InvalidRows, IReadOnlyList<ImportReviewRow> Rows)
{
    public int ChangedRows => Rows.Count(x => x.Disposition == ImportRowDisposition.Changed);
    public int CancellationUpdates => Rows.Count(x => x.Disposition == ImportRowDisposition.CancellationUpdate);
}
public sealed record ReservationImportResult(int CreatedReservations, int CreatedCharges, int StatementItems,
    int DuplicateRows, int HeldRows, int InvalidRows, int BatchesRecorded, string Message);

public sealed class ReservationImportException(string message, Exception? innerException = null) : Exception(message, innerException);

public interface IReservationImportService
{
    Task<ReservationImportReview> PrepareAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default);
    Task<ReservationImportResult> ImportAsync(Guid reviewID, CancellationToken cancellationToken = default);
    Task<ReservationImportResult> ImportAsync(Guid reviewID, IReadOnlyList<ImportRowDecision> decisions, CancellationToken cancellationToken = default);
    void DiscardReview();
}

public interface IReservationValidationService
{
    IReadOnlyList<string> Validate(ParsedReservation reservation);
}

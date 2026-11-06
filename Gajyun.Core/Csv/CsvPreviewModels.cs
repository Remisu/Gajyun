namespace Gajyun.Core.Csv;

public enum CsvFileEncoding { Auto, Utf8, Windows31J }

public sealed record CsvPreviewRecord(long RecordNumber, string[] Values, string RawRecord, bool IsValid, string? Message);

public sealed record CsvPreviewResult(
    string[] Headers, IReadOnlyList<CsvPreviewRecord> Records, long TotalRows, long ValidRows,
    long InvalidRows, string EncodingDescription, bool IsTruncated);

public sealed class CsvPreviewException(string message, Exception? innerException = null) : Exception(message, innerException);

public interface ICsvPreviewService
{
    Task<CsvPreviewResult> ReadAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default);
}

public enum ReservationCsvField
{
    Unmapped, ExternalReservationNumber, CheckInDate, CheckOutDate,
    ExternalRoomName, ExpectedGuestCount, Details, GuestName, BookedAt
}

public sealed record CsvColumnMapping(string SourceHeader, ReservationCsvField TargetField);

public interface ICsvReservationParser
{
    string ParserName { get; }
    bool Supports(IReadOnlyList<string> headers);
    // Implementations require a verified source sample and must preserve every original field.
    IReadOnlyList<string> ValidateMapping(IReadOnlyList<CsvColumnMapping> mappings);
    ReservationParseResult Parse(IReadOnlyDictionary<string, string> fields);
}

namespace Gajyun.Core.Models;

public sealed class ImportBatch
{
    public long ImportBatchID { get; set; }
    public int OTAID { get; set; }
    public string? SourceFileName { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? SourceFileHash { get; set; }
    public string? SourceEncoding { get; set; }
    public byte[]? SourceFileContent { get; set; }
    public string? ParserVersion { get; set; }
}

public sealed class ImportRow
{
    public long ImportRowID { get; set; }
    public long ImportBatchID { get; set; }
    public int SourceRowNumber { get; set; }
    public long? ReservationID { get; set; }
    public string? RawDetails { get; set; }
    public required string RawPayload { get; set; }
    public string ParseStatus { get; set; } = "Pending";
    public string? ParseError { get; set; }
    public string? SourceRowHash { get; set; }
}

public sealed class OTARoomMapping
{
    public int OTARoomMappingID { get; set; }
    public int? OTAID { get; set; }
    public int RoomID { get; set; }
    public required string ExternalRoomName { get; set; }
    public required string NormalizedName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

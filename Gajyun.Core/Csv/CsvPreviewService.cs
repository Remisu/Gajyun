using System.Globalization;
using System.Text;
using CsvHelper;

namespace Gajyun.Core.Csv;

public sealed class CsvPreviewService : ICsvPreviewService, ICsvImportFileReader
{
    public const int PreviewLimit = 200;
    public const int ImportRowLimit = 10_000;
    public const int ImportByteLimit = 25 * 1024 * 1024;

    public async Task<CsvPreviewResult> ReadAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await ReadStreamAsync(stream, encoding, delimiter, PreviewLimit, false, progress, cancellationToken);
    }

    public async Task<CsvImportFile> ReadForImportAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (file.Length > ImportByteLimit)
            throw new CsvPreviewException("Import files must not exceed 25 MiB. Split this export before importing.");
        var bytes = new byte[checked((int)file.Length)];
        await file.ReadExactlyAsync(bytes, cancellationToken);
        using var snapshot = new MemoryStream(bytes, writable: false);
        var result = await ReadStreamAsync(snapshot, encoding, delimiter, ImportRowLimit, true, progress, cancellationToken);
        return new CsvImportFile(bytes, Path.GetFileName(path), result);
    }

    private static async Task<CsvPreviewResult> ReadStreamAsync(Stream stream, CsvFileEncoding encoding, char delimiter,
        int captureLimit, bool enforceRowLimit, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        if (delimiter is not (',' or ';' or '\t'))
            throw new ArgumentException("Choose a comma, semicolon or tab delimiter.", nameof(delimiter));
        if (!Enum.IsDefined(encoding))
            throw new ArgumentOutOfRangeException(nameof(encoding));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var prefix = new byte[3];
        var prefixLength = await stream.ReadAtLeastAsync(prefix, 3, throwOnEndOfStream: false, cancellationToken);
        stream.Position = 0;
        var hasBom = prefixLength == 3 && prefix[0] == 0xEF && prefix[1] == 0xBB && prefix[2] == 0xBF;
        if (prefixLength >= 2 && ((prefix[0] == 0xFF && prefix[1] == 0xFE) || (prefix[0] == 0xFE && prefix[1] == 0xFF)))
            throw new CsvPreviewException("UTF-16 CSV is not supported. Export as UTF-8 or Windows-31J.");
        if (hasBom && encoding == CsvFileEncoding.Windows31J)
            throw new CsvPreviewException("This file has a UTF-8 BOM. Select UTF-8 or automatic detection.");

        var utf8 = new UTF8Encoding(false, true);
        var useUtf8 = encoding == CsvFileEncoding.Utf8 || hasBom;
        if (encoding == CsvFileEncoding.Auto && !hasBom)
            useUtf8 = await IsUtf8Async(stream, utf8, cancellationToken);
        stream.Position = hasBom ? 3 : 0;
        var selectedEncoding = useUtf8 ? utf8 : Encoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        var description = useUtf8 ? (hasBom ? "UTF-8 with BOM" : "UTF-8") : "Windows-31J (CP932)";
        if (encoding == CsvFileEncoding.Auto && !hasBom)
            description += " - inferred; verify Japanese text in the preview";

        using var reader = new StreamReader(stream, selectedEncoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        using var csv = CsvReader.Create(reader, new CsvReaderOptions
        {
            Separator = delimiter,
            HasHeader = true,
            MaxRecordSize = 1_000_000,
            IncludeValuesInExceptions = false,
            FormatProvider = CultureInfo.InvariantCulture,
            TrackLineNumbers = true,
            MalformedData = CsvMalformedDataHandling.Throw,
            FieldCountMismatch = CsvFieldCountHandling.Allow
        }, leaveOpen: true);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await csv.ReadHeaderAsync(cancellationToken);
            var headers = csv.Headers.ToArray();
            if (headers.Length == 0)
                throw new CsvPreviewException("The CSV file is empty or has no header row.");
            if (headers.Any(string.IsNullOrWhiteSpace))
                throw new CsvPreviewException("Every CSV column must have a non-empty header.");
            if (headers.Sum(x => Encoding.UTF8.GetByteCount(x)) > 1_000_000)
                throw new CsvPreviewException("The CSV header exceeds the preview limit of 1,000,000 UTF-8 bytes.");
            if (headers.Distinct(StringComparer.Ordinal).Count() != headers.Length)
                throw new CsvPreviewException("The CSV contains duplicate column headers. Rename them before mapping.");

            var records = new List<CsvPreviewRecord>();
            long total = 0, valid = 0, invalid = 0;
            while (await csv.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (csv.RawRecord.Length > 1_000_000)
                    throw new CsvPreviewException($"CSV record {total + 1} exceeds the preview limit of 1,000,000 UTF-8 bytes.");
                total++;
                if (enforceRowLimit && total > ImportRowLimit)
                    throw new CsvPreviewException("Import files must not exceed 10,000 data records. No rows were saved.");
                var isValid = csv.FieldCount == headers.Length;
                if (isValid) valid++; else invalid++;
                if (records.Count < captureLimit)
                {
                    var values = Enumerable.Range(0, csv.FieldCount).Select(csv.GetString).ToArray();
                    records.Add(new CsvPreviewRecord(total, values, Encoding.UTF8.GetString(csv.RawRecord), isValid,
                        isValid ? null : $"Expected {headers.Length} fields; found {csv.FieldCount}."));
                }
                if (total % 500 == 0) progress?.Report(total);
            }
            progress?.Report(total);
            return new CsvPreviewResult(headers, records.AsReadOnly(), total, valid, invalid, description, total > captureLimit);
        }
        catch (DecoderFallbackException exception)
        {
            throw new CsvPreviewException("The file contains bytes invalid for the selected encoding. Try UTF-8 or Windows-31J explicitly.", exception);
        }
        catch (CsvFormatException exception)
        {
            throw new CsvPreviewException($"Malformed CSV near physical line {csv.LineNumber}. Check quoting, delimiter and record size (maximum 1,000,000 UTF-8 bytes).", exception);
        }
    }

    private static async Task<bool> IsUtf8Async(Stream stream, Encoding utf8, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, utf8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var buffer = new char[16384];
        try
        {
            while (await reader.ReadAsync(buffer.AsMemory(), cancellationToken) != 0)
                cancellationToken.ThrowIfCancellationRequested();
            return true;
        }
        catch (DecoderFallbackException)
        {
            // Automatic detection retries strict CP932; explicit encodings never silently fall back.
            return false;
        }
    }
}

using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gajyun.Core.Csv;
using Gajyun.Core.Services;
using Gajyun.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace Gajyun.Desktop.ViewModels;

public sealed record EncodingChoice(CsvFileEncoding Value, string Label);
public sealed record DelimiterChoice(char Value, string Label);

public partial class ColumnMappingViewModel(string header) : ObservableObject
{
    public string Header { get; } = header;
    public IReadOnlyList<ReservationCsvField> AvailableFields { get; } = Enum.GetValues<ReservationCsvField>();
    [ObservableProperty] private ReservationCsvField targetField;
}

public partial class CsvImportViewModel(ICsvPreviewService previewService, IFileDialogService fileDialog,
    IRoomSettingsService roomSettings, ILogger<CsvImportViewModel> logger, IReservationImportService importService) : ObservableObject
{
    public IReadOnlyList<EncodingChoice> Encodings { get; } =
    [new(CsvFileEncoding.Auto, "Auto (strict UTF-8, then Windows-31J)"), new(CsvFileEncoding.Utf8, "UTF-8"), new(CsvFileEncoding.Windows31J, "Windows-31J / CP932")];
    public IReadOnlyList<DelimiterChoice> Delimiters { get; } = [new(',', "Comma"), new(';', "Semicolon"), new('\t', "Tab")];
    public ObservableCollection<OtaOption> OTAs { get; } = [new(null, "Generic preview - no OTA mapping")];
    public ObservableCollection<ColumnMappingViewModel> ColumnMappings { get; } = [];
    public ObservableCollection<ImportReviewRowViewModel> ReviewRows { get; } = [];
    public IEnumerable<ImportReviewRowViewModel> NewBookings => ReviewRows.Where(x => x.Row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled);
    public IEnumerable<ImportReviewRowViewModel> ChangedBookings => ReviewRows.Where(x => x.Row.Disposition == ImportRowDisposition.Changed);
    public IEnumerable<ImportReviewRowViewModel> CancelledBookings => ReviewRows.Where(x => x.Row.Disposition == ImportRowDisposition.CancellationUpdate);
    public IEnumerable<ImportReviewRowViewModel> Questions => ReviewRows.Where(x => x.HasCustomerQuestion || x.HasRoomQuestion || x.Row.Disposition is ImportRowDisposition.Held or ImportRowDisposition.Invalid);
    private ReservationImportReview? review;
    public string ImportExplanation => "Agoda and conflicting prices remain held. Deposits are not cash receipts. Breakfast-inclusive prices leave tax pending. Staff-managed fields are preserved.";
    public bool CanImport => !IsLoading && ReviewAcknowledged && review is not null && !ReviewRows.Any(x => x.IsUnresolved) &&
        (review.ReadyRows + review.CancelledRows + review.DuplicateRows + review.ChangedRows + review.CancellationUpdates > 0 || ReviewRows.Any(x => x.SelectedRoom is not null));

    [ObservableProperty] private bool reviewAcknowledged;
    [ObservableProperty] private string importSummary = "No reservation validation has run. No data has been saved.";
    [ObservableProperty] private string importResults = "";
    [ObservableProperty] private EncodingChoice selectedEncoding = new(CsvFileEncoding.Auto, "Auto (strict UTF-8, then Windows-31J)");
    [ObservableProperty] private DelimiterChoice selectedDelimiter = new(',', "Comma");
    [ObservableProperty] private OtaOption selectedOta = new(null, "Generic preview - no OTA mapping");
    [ObservableProperty] private string selectedFile = "No file selected";
    [ObservableProperty] private string encodingInformation = "Not read yet";
    [ObservableProperty] private DataView? preview;
    [ObservableProperty] private long totalRows;
    [ObservableProperty] private long validRows;
    [ObservableProperty] private long invalidRows;
    [ObservableProperty] private string status = "Choose a CSV file. Row counts reflect CSV structure only, not validated reservations.";
    [ObservableProperty] private string otaStatus = "OTA selection is optional for preview. No OTA-specific parser is configured.";
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool hasPreview;
    private string? selectedPath;

    public string SelectedPath => selectedPath ?? "";

    private bool CanSelectFile() => !IsLoading;

    [RelayCommand(CanExecute = nameof(CanSelectFile), IncludeCancelCommand = true)]
    private async Task SelectFileAsync(CancellationToken cancellationToken)
    {
        var path = fileDialog.SelectCsvFile();
        if (path is null) return;
        ClearReview();
        selectedPath = path;
        SelectedFile = Path.GetFileName(path);
        OnPropertyChanged(nameof(SelectedPath));
        ReloadCommand.NotifyCanExecuteChanged();
        await LoadPreviewAsync(cancellationToken);
        if (HasPreview) await ValidateFileAsync(cancellationToken);
    }

    private bool CanReload() => selectedPath is not null && !IsLoading;

    [RelayCommand(CanExecute = nameof(CanReload), IncludeCancelCommand = true)]
    private Task ReloadAsync(CancellationToken cancellationToken) => LoadPreviewAsync(cancellationToken);

    [RelayCommand]
    private async Task RefreshOtasAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await roomSettings.LoadAsync(cancellationToken);
            var selectedID = SelectedOta.OTAID;
            OTAs.Clear();
            OTAs.Add(new(null, "Generic preview - no OTA mapping"));
            foreach (var ota in snapshot.OTAs) OTAs.Add(ota);
            SelectedOta = OTAs.FirstOrDefault(x => x.OTAID == selectedID) ?? OTAs[0];
            OtaStatus = "Preview OTA selector only. The verified combined export reads its source OTA from 予約サイト名 on each row; Agoda pricing remains held for review.";
        }
        catch (SettingsOperationException exception)
        {
            logger.LogWarning("OTA list unavailable: {Message}", exception.Message);
            OtaStatus = exception.Message + " Generic preview remains available.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OtaStatus = "OTA list refresh cancelled. Generic preview remains available.";
        }
    }

    partial void OnSelectedEncodingChanged(EncodingChoice value) => InvalidatePreview();
    partial void OnSelectedDelimiterChanged(DelimiterChoice value) => InvalidatePreview();
    partial void OnReviewAcknowledgedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanImport));
        ImportCommand.NotifyCanExecuteChanged();
    }
    partial void OnIsLoadingChanged(bool value)
    {
        SelectFileCommand.NotifyCanExecuteChanged();
        ReloadCommand.NotifyCanExecuteChanged();
        ValidateFileCommand.NotifyCanExecuteChanged();
        ImportCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanImport));
    }

    [RelayCommand(CanExecute = nameof(CanReload), IncludeCancelCommand = true)]
    private async Task ValidateFileAsync(CancellationToken cancellationToken)
    {
        if (selectedPath is null) return;
        ClearReview();
        IsLoading = true;
        ImportSummary = "Validating all source rows, room mappings, nightly prices and existing bookings...";
        try
        {
            var path = selectedPath;
            var encoding = SelectedEncoding.Value;
            var delimiter = SelectedDelimiter.Value;
            var result = await Task.Run(() => importService.PrepareAsync(path, encoding, delimiter, cancellationToken: cancellationToken), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            review = result;
            foreach (var row in result.Rows) ReviewRows.Add(new(row, ReviewChoicesChanged));
            ImportSummary = $"{result.ReadyRows + result.CancelledRows} new · {result.ChangedRows} changed · {result.CancellationUpdates} previously active cancellations · " +
                $"{result.DuplicateRows} unchanged · {result.HeldRows + result.InvalidRows} held/invalid";
            Status = "Review changes and the Questions tab, then approve this batch.";
            NotifyReviewLists();
        }
        catch (ReservationImportException exception) { ImportSummary = exception.Message; }
        catch (CsvPreviewException exception) { ImportSummary = exception.Message; }
        catch (IOException) { ImportSummary = "The source CSV could not be read. Check that the selected file exists and is not locked."; }
        catch (UnauthorizedAccessException) { ImportSummary = "Access to the source file was denied. Select a readable local file."; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            importService.DiscardReview();
            ImportSummary = "Import validation cancelled. No data was saved.";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanImport));
            ImportCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport), IncludeCancelCommand = true)]
    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        if (review is null) return;
        var id = review.ReviewID;
        IsLoading = true;
        ImportResults = "Saving reviewed rows in one database transaction...";
        try
        {
            var decisions = ReviewRows.Select(x => x.Decision).ToArray();
            var result = await importService.ImportAsync(id, decisions, cancellationToken);
            ClearReview();
            ImportResults = result.Message;
            Status = "Import finished. Select another file or review again.";
            await RefreshOtasAsync(cancellationToken);
        }
        catch (ReservationImportException exception)
        {
            ClearReview();
            ImportResults = exception.Message;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ClearReview();
            ImportResults = "Import cancelled. Review again to confirm saved outcomes before retrying.";
        }
        finally { IsLoading = false; }
    }

    private void ClearReview()
    {
        importService.DiscardReview();
        review = null;
        ReviewAcknowledged = false;
        ReviewRows.Clear();
        NotifyReviewLists();
        ImportSummary = "Choose a CSV file to review bookings.";
        ImportResults = "";
        OnPropertyChanged(nameof(CanImport));
        ImportCommand.NotifyCanExecuteChanged();
    }

    private void ReviewChoicesChanged()
    {
        ReviewAcknowledged = false;
        OnPropertyChanged(nameof(CanImport));
        ImportCommand.NotifyCanExecuteChanged();
    }

    private void NotifyReviewLists()
    {
        OnPropertyChanged(nameof(NewBookings)); OnPropertyChanged(nameof(ChangedBookings));
        OnPropertyChanged(nameof(CancelledBookings)); OnPropertyChanged(nameof(Questions));
    }

    [RelayCommand]
    private void CancelWork()
    {
        SelectFileCommand.Cancel(); ReloadCommand.Cancel(); ValidateFileCommand.Cancel(); ImportCommand.Cancel();
    }

    private void InvalidatePreview()
    {
        ClearReview();
        Preview = null;
        ColumnMappings.Clear();
        TotalRows = ValidRows = InvalidRows = 0;
        HasPreview = false;
        EncodingInformation = "Not read with the selected options";
        Status = "Options changed. Reload the selected CSV to preview it with these options.";
    }

    private async Task LoadPreviewAsync(CancellationToken cancellationToken)
    {
        if (selectedPath is null) return;
        ClearReview();
        IsLoading = true;
        ReloadCommand.NotifyCanExecuteChanged();
        Preview = null;
        HasPreview = false;
        ColumnMappings.Clear();
        TotalRows = ValidRows = InvalidRows = 0;
        EncodingInformation = "Reading...";
        Status = "Reading CSV and counting records...";
        var path = selectedPath;
        var encoding = SelectedEncoding.Value;
        var delimiter = SelectedDelimiter.Value;
        var progress = new Progress<long>(rows => { if (IsLoading) Status = $"Read {rows:N0} records..."; });
        try
        {
            var result = await Task.Run(() => previewService.ReadAsync(path, encoding, delimiter, progress, cancellationToken), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var table = new DataTable();
            table.Columns.Add("RecordNumber", typeof(long)).Caption = "Record";
            table.Columns.Add("Validation", typeof(string)).Caption = "CSV structure";
            var columnCount = Math.Max(result.Headers.Length, result.Records.Select(x => x.Values.Length).DefaultIfEmpty().Max());
            for (var index = 0; index < columnCount; index++)
                table.Columns.Add($"C{index}", typeof(string)).Caption = index < result.Headers.Length ? result.Headers[index] : $"Extra field {index - result.Headers.Length + 1}";
            table.Columns.Add("RawRecord", typeof(string)).Caption = "Original CSV record";
            foreach (var record in result.Records)
            {
                var row = table.NewRow();
                row["RecordNumber"] = record.RecordNumber;
                row["Validation"] = record.IsValid ? "Valid structure (reservation not validated)" : record.Message ?? "Invalid structure";
                for (var index = 0; index < record.Values.Length; index++) row[$"C{index}"] = record.Values[index];
                row["RawRecord"] = record.RawRecord;
                table.Rows.Add(row);
            }
            Preview = table.DefaultView;
            foreach (var header in result.Headers) ColumnMappings.Add(new(header));
            EncodingInformation = result.EncodingDescription;
            TotalRows = result.TotalRows;
            ValidRows = result.ValidRows;
            InvalidRows = result.InvalidRows;
            HasPreview = true;
            Status = $"Read {result.TotalRows:N0} records. Showing {result.Records.Count:N0}{(result.IsTruncated ? $" (preview limited to {CsvPreviewService.PreviewLimit})" : "")}. " +
                (result.InvalidRows > 0 ? $"{result.InvalidRows:N0} record(s) have a different field count. " : "") +
                "Reservation validation has not run. No data has been saved.";
        }
        catch (CsvPreviewException exception)
        {
            logger.LogWarning("CSV preview rejected: {Message}", exception.Message);
            Status = exception.Message + " No data was saved.";
            EncodingInformation = "Preview failed";
        }
        catch (IOException exception)
        {
            logger.LogWarning("CSV preview I/O failure: {ErrorType}", exception.GetType().Name);
            Status = "The CSV could not be read. Check that the file exists and is not locked, then retry.";
            EncodingInformation = "Preview failed";
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("CSV preview access denied.");
            Status = "Access to this CSV was denied. Select a readable local file.";
            EncodingInformation = "Preview failed";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Status = "CSV loading cancelled. No data was saved.";
            EncodingInformation = "Cancelled";
        }
        finally
        {
            IsLoading = false;
            ReloadCommand.NotifyCanExecuteChanged();
        }
    }
}

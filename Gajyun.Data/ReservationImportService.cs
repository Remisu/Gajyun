using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gajyun.Core.Csv;
using Gajyun.Core.Models;
using Gajyun.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gajyun.Data;

public sealed class ReservationImportService(
    IDbContextFactory<GajyunDbContext> contextFactory, ICsvImportFileReader fileReader,
    ICsvReservationParser parser, IRoomMappingService roomMapper, ILogger<ReservationImportService> logger) : IReservationImportService
{
    private const string ParserVersion = "combined-jp-v2";
    private readonly SemaphoreSlim operationLock = new(1, 1);
    private PreparedImport? prepared;
    private long reviewGeneration;

    public void DiscardReview()
    {
        Interlocked.Increment(ref reviewGeneration);
        Interlocked.Exchange(ref prepared, null);
    }

    public async Task<ReservationImportReview> PrepareAsync(string path, CsvFileEncoding encoding, char delimiter,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        await operationLock.WaitAsync(cancellationToken);
        try
        {
            DiscardReview();
            var generation = Volatile.Read(ref reviewGeneration);
            var file = await fileReader.ReadForImportAsync(path, encoding, delimiter, progress, cancellationToken);
            if (!parser.Supports(file.Content.Headers))
                throw new ReservationImportException("This file does not match the verified combined Japanese export. Generic column selections cannot enable import.");
            if (file.Content.TotalRows == 0)
                throw new ReservationImportException("The file contains no reservation records.");
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var mappings = await LoadMappingsAsync(db, cancellationToken);
            var otas = await db.OTAs.AsNoTracking().ToDictionaryAsync(x => x.OTAName, cancellationToken);
            var rooms = await db.Rooms.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.RoomID).ToArrayAsync(cancellationToken);
            var rows = new List<PreparedRow>();
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in file.Content.Records)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fields = file.Content.Headers.Select((header, index) => new KeyValuePair<string, string>(header, index < record.Values.Length ? record.Values[index] : ""))
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                var otaName = fields["予約サイト名"].Trim();
                if (!CombinedExportReservationParser.IsKnownSource(otaName))
                    throw new ReservationImportException($"Record {record.RecordNumber} has an unverified source OTA. The entire file must be reviewed before import; no data was saved.");
                var rawPayload = JsonSerializer.Serialize(new { Headers = file.Content.Headers, Values = record.Values, record.RawRecord });
                var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fields.OrderBy(x => x.Key, StringComparer.Ordinal)))));
                var parsed = record.IsValid ? parser.Parse(fields) : new ReservationParseResult(null, record.Message ?? "CSV field count is invalid.");
                var row = new PreparedRow(checked((int)record.RecordNumber), otaName, rawPayload, fingerprint, fields["詳細"], parsed.Reservation,
                    null, ImportRowDisposition.Held, parsed.Message)
                {
                    SourceNumber = fields["予約番号"].Trim(), SourceGuestName = fields["宿泊者氏名"].Trim(), SourceFields = fields,
                    SourceBookedAt = DateTime.TryParseExact(fields["予約日時"].Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var booked) ? booked : null
                };
                if (!record.IsValid) row = row with { Disposition = ImportRowDisposition.Invalid };
                else if (parsed.Reservation is ParsedReservation reservation)
                {
                    var match = ResolveRoom(reservation, otas.GetValueOrDefault(otaName)?.OTAID, mappings);
                    row = row with { RoomID = match.RoomID, NeedsRoomChoice = match.Status != RoomMatchStatus.Matched,
                        Disposition = reservation.IsCancelled ? ImportRowDisposition.Cancelled : ImportRowDisposition.Ready,
                        Message = match.Status == RoomMatchStatus.Matched ? parsed.Message : match.Message };
                    if (otas.TryGetValue(otaName, out var ota))
                    {
                        if (!ota.IsActive) row = row with { Disposition = ImportRowDisposition.Held, NeedsRoomChoice = false, Message = "Source OTA is inactive." };
                        else row = await ClassifyExistingAsync(db, row, ota.OTAID, cancellationToken);
                    }
                    if (row.NeedsRoomChoice && row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled)
                        row = row with { Disposition = ImportRowDisposition.Held };
                }
                var key = otaName + "\u001f" + fields["予約番号"].Trim();
                if (!keys.Add(key))
                    throw new ReservationImportException($"Record {record.RecordNumber} repeats an OTA booking identifier in this file. Resolve duplicate source rows before importing; no rows were saved.");
                rows.Add(row);
            }
            rows = rows.OrderBy(x => x.Reservation?.BookedAt ?? x.SourceBookedAt ?? DateTime.MaxValue).ThenBy(x => x.Number).ToList();
            var reviewRows = new List<ImportReviewRow>();
            foreach (var row in rows)
            {
                var source = row.Reservation;
                var options = row.NeedsRoomChoice ? rooms.Select(x => new ImportRoomOption(x.RoomID, x.RoomName)).ToArray() : [];
                var comparison = row.NeedsRoomChoice || row.Disposition is ImportRowDisposition.Held or ImportRowDisposition.Invalid or ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate
                    ? await BuildComparisonAsync(db, row, otas.GetValueOrDefault(row.OTAName)?.OTAID, cancellationToken)
                    : (HasExisting: row.ExistingID is not null, Fields: (IReadOnlyList<ReservationComparisonField>)Array.Empty<ReservationComparisonField>());
                reviewRows.Add(new ImportReviewRow(row.Number, row.OTAName, row.Disposition, row.Message,
                    source?.Nights.Count ?? 0, source?.Nights.Sum(x => x.RoomPrice) ?? 0)
                {
                    ReservationNumber = source?.ExternalNumber ?? row.SourceNumber, GuestName = source?.GuestName ?? row.SourceGuestName, BookedAt = source?.BookedAt ?? row.SourceBookedAt,
                    CheckIn = source?.CheckIn, CheckOut = source?.CheckOut, RoomName = rooms.FirstOrDefault(x => x.RoomID == row.RoomID)?.RoomName ?? source?.RoomMaster ?? "",
                    HasExistingReservation = comparison.HasExisting, ComparisonFields = comparison.Fields, RoomChoices = options
                });
            }
            var id = Guid.NewGuid();
            var review = new ReservationImportReview(id, rows.Count,
                rows.Count(x => x.Disposition == ImportRowDisposition.Ready), rows.Count(x => x.Disposition == ImportRowDisposition.Cancelled),
                rows.Count(x => x.Disposition == ImportRowDisposition.Duplicate), rows.Count(x => x.Disposition == ImportRowDisposition.Held),
                rows.Count(x => x.Disposition == ImportRowDisposition.Invalid), reviewRows.AsReadOnly());
            Interlocked.Exchange(ref prepared, new PreparedImport(id, file, rows.AsReadOnly(), review.Rows));
            if (Volatile.Read(ref reviewGeneration) != generation)
            {
                DiscardReview();
                throw new ReservationImportException("The selected file or options changed during validation. Validate the current file again; no data was saved.");
            }
            return review;
        }
        catch (SqlException exception) { throw DatabaseError(exception.Number); }
        finally { operationLock.Release(); }
    }

    public Task<ReservationImportResult> ImportAsync(Guid reviewID, CancellationToken cancellationToken = default) =>
        ImportAsync(reviewID, [], cancellationToken);

    public async Task<ReservationImportResult> ImportAsync(Guid reviewID, IReadOnlyList<ImportRowDecision> decisions, CancellationToken cancellationToken = default)
    {
        await operationLock.WaitAsync(cancellationToken);
        try
        {
            var plan = prepared;
            if (plan is null || plan.ID != reviewID)
                throw new ReservationImportException("The import review expired or the file options changed. Validate the file again before importing.");
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await db.Database.ExecuteSqlRawAsync("""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource = N'Gajyun.ReservationImport',
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
                IF @result < 0 THROW 51001, 'Another reservation import is running. Please retry.', 1;
                """, cancellationToken);
            var mappings = await LoadMappingsAsync(db, cancellationToken);
            var statuses = await db.BookingStatuses.Where(x => x.StatusName == "Confirmed" || x.StatusName == "Cancelled")
                .ToDictionaryAsync(x => x.StatusName, cancellationToken);
            if (statuses.Count != 2)
                throw new ReservationImportException("Required booking statuses are missing. Initialize the database before importing.");
            var policies = await db.TaxPolicies.AsNoTracking().ToArrayAsync(cancellationToken);
            var roomDefaults = await db.Rooms.Where(x => x.IsActive).ToDictionaryAsync(x => x.RoomID, x => x.DefaultTaxPolicyID, cancellationToken);
            var fileHash = Convert.ToHexString(SHA256.HashData(plan.File.OriginalBytes));
            if (decisions.Select(x => x.RowNumber).Distinct().Count() != decisions.Count || decisions.Any(x => !plan.Rows.Any(row => row.Number == x.RowNumber)))
                throw new ReservationImportException("The review choices are invalid. Review the file again.");
            var choices = decisions.ToDictionary(x => x.RowNumber);
            var resolvedRooms = plan.Rows.Where(x => x.Reservation is not null &&
                    (x.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled or ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate || choices.GetValueOrDefault(x.Number)?.RoomID is not null))
                .Select(x => new { x.OTAName, Name = RoomNameNormalizer.Normalize(x.Reservation!.ExternalRoomName), RoomID = choices.GetValueOrDefault(x.Number)?.RoomID ?? x.RoomID });
            if (resolvedRooms.GroupBy(x => new { x.OTAName, x.Name }).Any(x => x.Where(y => y.RoomID is not null).Select(y => y.RoomID).Distinct().Count() > 1))
                throw new ReservationImportException("The same OTA room name has conflicting physical-room choices. Choose one consistent mapping before importing.");
            var customers = await db.Customers.AsNoTracking().Select(x => new { x.CustomerID, x.FullName }).ToArrayAsync(cancellationToken);
            var customersByName = customers.OrderBy(x => x.CustomerID).GroupBy(x => RoomNameNormalizer.Normalize(x.FullName), StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First().CustomerID, StringComparer.Ordinal);
            var methodIDs = await db.PaymentMethods.ToDictionaryAsync(x => x.MethodName, x => x.PaymentMethodID, cancellationToken);
            var created = 0;
            var updated = 0;
            var charges = 0;
            var statements = 0;
            var duplicates = 0;
            var held = 0;
            var invalid = 0;
            var batches = 0;
            var processed = 0;
            var otas = await db.OTAs.ToDictionaryAsync(x => x.OTAName, cancellationToken);
            var batchByOTA = new Dictionary<string, ImportBatch>(StringComparer.Ordinal);
            foreach (var otaName in plan.Rows.Select(x => x.OTAName).Distinct(StringComparer.Ordinal))
            {
                if (!otas.TryGetValue(otaName, out var sourceOTA) || !sourceOTA.IsActive)
                    throw new ReservationImportException("A source OTA is missing or inactive. Initialize and review again.");
                var batch = new ImportBatch
                {
                    OTAID = sourceOTA.OTAID, SourceFileName = plan.File.FileName, SourceFileHash = fileHash,
                    SourceEncoding = plan.File.Content.EncodingDescription, SourceFileContent = plan.File.OriginalBytes, ParserVersion = ParserVersion
                };
                db.ImportBatches.Add(batch); batchByOTA.Add(otaName, batch); batches++;
            }
            await db.SaveChangesAsync(cancellationToken);
            var groups = await PrepareGroupsAsync(db, plan.Rows, otas, cancellationToken);
            foreach (var original in plan.Rows)
            {
                    cancellationToken.ThrowIfCancellationRequested();
                    var ota = otas[original.OTAName];
                    var batch = batchByOTA[original.OTAName];
                    var choice = choices.GetValueOrDefault(original.Number);
                    var row = original;
                    var reviewRow = plan.ReviewRows.Single(x => x.RowNumber == row.Number);
                    if (choice?.RoomID is int selectedRoom)
                    {
                        if (!reviewRow.RoomChoices.Any(x => x.RoomID == selectedRoom) || !roomDefaults.ContainsKey(selectedRoom))
                            throw new ReservationImportException("The selected room is not an allowed active room. Review again.");
                        row = row with { RoomID = selectedRoom, NeedsRoomChoice = false,
                            Disposition = row.ExistingID is null ? row.Reservation!.IsCancelled ? ImportRowDisposition.Cancelled : ImportRowDisposition.Ready : row.Disposition };
                    }
                    if (row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled or ImportRowDisposition.Duplicate or ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate)
                    {
                        var check = await ClassifyExistingAsync(db, row, ota.OTAID, cancellationToken);
                        if (check.Disposition != row.Disposition || check.ExistingID != row.ExistingID || check.ExistingSnapshot != row.ExistingSnapshot)
                            throw new ReservationImportException("A booking changed after review. Review again; nothing was committed.");
                        if (row.NeedsRoomChoice) throw new ReservationImportException("Choose which physical room is correct before applying this changed booking.");
                    }
                    long? reservationID = null;
                    if (row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled)
                    {
                        var source = row.Reservation ?? throw new InvalidOperationException("A ready import row has no parsed reservation.");
                        var match = ResolveRoom(source, ota.OTAID, mappings);
                        if (row.RoomID is not int roomID || !roomDefaults.ContainsKey(roomID) ||
                            choice?.RoomID is null && (match.RoomID != row.RoomID || match.Status != RoomMatchStatus.Matched))
                            throw new ReservationImportException("Room mappings changed after review. Review again; nothing was committed.");
                        await SaveSourceMappingAsync(db, ota.OTAID, source.ExternalRoomName, roomID, choice?.RoomID is not null, cancellationToken);
                        var reservation = new Reservation
                        {
                            OTAID = ota.OTAID, ExternalReservationNumber = source.ExternalNumber,
                            BookingStatusID = statuses[source.IsCancelled ? "Cancelled" : "Confirmed"].BookingStatusID,
                            BookedAt = source.BookedAt, ModifiedAtSource = source.ModifiedAtSource,
                            CheckInDate = source.CheckIn, CheckOutDate = source.CheckOut, ExpectedGuestCount = source.GuestCount,
                            IsModified = source.IsModified, ConsecutiveStayInfo = source.ConsecutiveInfo,
                            RoomRateInfo = source.RoomRateInfo, BookingDetails = source.Details,
                            SourcePaymentMethodID = source.SourcePaymentMethod is string method ? methodIDs[method] : null,
                            GroupID = groups.GetValueOrDefault(GroupKey(source, ota.OTAID) ?? "")
                        };
                        db.Reservations.Add(reservation);
                        var customerID = await ResolveCustomerAsync(db, source, customersByName, cancellationToken);
                        await db.SaveChangesAsync(cancellationToken);
                        reservationID = reservation.ReservationID;
                        db.ReservationCustomers.Add(new ReservationCustomer { ReservationID = reservation.ReservationID, CustomerID = customerID, IsLeadGuest = true });
                        if (!source.IsCancelled) await LinkStayAsync(db, reservation, customerID, cancellationToken);
                        if (!source.IsCancelled)
                        {
                            db.ReservationAccommodations.Add(new ReservationAccommodation
                            {
                                ReservationID = reservation.ReservationID, RoomID = roomID, BedID = null,
                                StartDate = source.CheckIn, EndDate = source.CheckOut,
                                Notes = "CSV room-level association only. No individual bed was supplied or assigned."
                            });
                            foreach (var night in source.Nights)
                            {
                                db.ReservationCharges.Add(new ReservationCharge
                                {
                                    ReservationID = reservation.ReservationID, ChargeType = "Accommodation", ServiceDate = night.ServiceDate,
                                    Description = night.Description, Quantity = 1m, UnitPrice = night.RoomPrice, CurrencyCode = "JPY",
                                    SourceLineKey = $"{ParserVersion}:night:{night.ServiceDate:yyyy-MM-dd}"
                                });
                                var legalPolicies = policies.Where(x => x.ValidFrom <= night.ServiceDate && (x.ValidTo is null || night.ServiceDate < x.ValidTo)).ToArray();
                                db.ReservationTaxes.Add(new ReservationTax
                                {
                                    ReservationID = reservation.ReservationID, RoomID = roomID, ServiceDate = night.ServiceDate,
                                    GuestCount = source.GuestCount, AppliedTaxPolicyID = roomDefaults[roomID],
                                    LegalTaxPolicyID = legalPolicies.Length == 1 ? legalPolicies[0].TaxPolicyID : null,
                                    CalculationStatus = "PendingReview", Notes = "Breakfast-inclusive total-room price. Accommodation-only taxable base and charged tax are not established. No tax charge was created."
                                });
                                charges++;
                            }
                            for (var index = 0; index < source.Statements.Count; index++)
                            {
                                var statement = source.Statements[index];
                                db.OTAStatementItems.Add(new OTAStatementItem
                                {
                                    ReservationID = reservation.ReservationID, RawLabel = statement.Label, RawText = statement.RawText,
                                    Amount = statement.Amount, CurrencyCode = "JPY", Classification = "Unclassified",
                                    SourceLineKey = $"{ParserVersion}:statement:{index}",
                                    ResolutionNotes = "Source statement only; not treated as an actual payment, automatic commission charge or revenue."
                                });
                                statements++;
                            }
                        }
                        created++;
                    }
                    else if (row.Disposition is ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate)
                    {
                        var source = row.Reservation!;
                        if (row.RoomID is not int roomID || !roomDefaults.ContainsKey(roomID)) throw new ReservationImportException("Choose an active room for the changed booking.");
                        var match = ResolveRoom(source, ota.OTAID, mappings);
                        if (choice?.RoomID is null && (match.Status != RoomMatchStatus.Matched || match.RoomID != roomID))
                            throw new ReservationImportException("Room mapping changed after review. Review again.");
                        await SaveSourceMappingAsync(db, ota.OTAID, source.ExternalRoomName, roomID, choice?.RoomID is not null, cancellationToken);
                        var existing = await db.Reservations.SingleAsync(x => x.ReservationID == row.ExistingID, cancellationToken);
                        await ApplyUpdateAsync(db, existing, source, roomID, statuses[source.IsCancelled ? "Cancelled" : "Confirmed"].BookingStatusID,
                            source.SourcePaymentMethod is string method ? methodIDs[method] : null, roomDefaults[roomID], policies, cancellationToken);
                        reservationID = existing.ReservationID;
                        updated++;
                    }
                    else if (row.Disposition == ImportRowDisposition.Duplicate)
                    {
                        reservationID = await db.Reservations.Where(x => x.OTAID == ota.OTAID && x.ExternalReservationNumber == row.Reservation!.ExternalNumber)
                            .Select(x => (long?)x.ReservationID).SingleAsync(cancellationToken);
                        duplicates++;
                    }
                    else if (row.Disposition == ImportRowDisposition.Invalid) invalid++;
                    else held++;
                    db.ImportRows.Add(new ImportRow
                    {
                        ImportBatchID = batch.ImportBatchID, SourceRowNumber = row.Number, ReservationID = reservationID,
                        RawDetails = row.RawDetails, RawPayload = row.RawPayload, SourceRowHash = row.Fingerprint,
                        ParseStatus = row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled or ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate ? "Processed"
                            : row.Disposition == ImportRowDisposition.Invalid ? "Failed" : "Skipped",
                        ParseError = (row.Message.Length <= 1700 ? row.Message : row.Message[..1700] + " [Summary shortened; compare audited source versions for remaining differences.]") +
                            (row.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled ? " Customer resolved by normalized name under the staff-approved same-name rule." : "") +
                            (choice?.RoomID is not null ? $" Physical room {choice.RoomID} explicitly confirmed." : "")
                    });
                    await db.SaveChangesAsync(cancellationToken);
                    processed++;
            }
            if (Volatile.Read(ref prepared)?.ID != reviewID)
                throw new ReservationImportException("The review expired during import. Review again; nothing was committed.");
            await AssignExistingGroupsAsync(db, groups, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            DiscardReview();
            logger.LogInformation("Reservation import committed: {Created} bookings, {Charges} nightly charges, {Statements} source statements, {Duplicates} duplicates, {Held} held, {Invalid} invalid, {Rows} audited rows.",
                created, charges, statements, duplicates, held, invalid, processed);
            return new ReservationImportResult(created, charges, statements, duplicates, held, invalid, batches,
                $"Imported {created} new bookings; applied {updated} approved changes/cancellations. Unchanged {duplicates}; held {held}; invalid {invalid}. All {processed} rows audited. Staff notes, payments, extras, groups and check-in timestamps preserved.");
        }
        catch (SqlException exception) { throw DatabaseError(exception.Number); }
        catch (DbUpdateException)
        {
            logger.LogError("Reservation import failed a database constraint; no import changes were committed. Guest payloads are not logged.");
            throw new ReservationImportException("The import violated a database constraint. The transaction was rolled back. Refresh and validate before retrying.");
        }
        finally { operationLock.Release(); }
    }

    private static async Task<(bool HasExisting, IReadOnlyList<ReservationComparisonField> Fields)> BuildComparisonAsync(
        GajyunDbContext db, PreparedRow row, int? otaID, CancellationToken ct)
    {
        var source = row.Reservation;
        var existing = otaID is null || row.SourceNumber.Length == 0 ? null :
            await db.Reservations.AsNoTracking().SingleOrDefaultAsync(x => x.OTAID == otaID && x.ExternalReservationNumber == row.SourceNumber, ct);
        var fields = new List<ReservationComparisonField>();
        string Raw(string header) => row.SourceFields.GetValueOrDefault(header) ?? "";
        static string Display(object? value) => value switch
        {
            null => "(not supplied)",
            string text when string.IsNullOrWhiteSpace(text) => "(not supplied)",
            DateOnly date => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            DateTime time => time.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            decimal money => money.ToString("N2", System.Globalization.CultureInfo.InvariantCulture),
            IFormattable number => number.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()!
        };
        void Add(string label, object? current, object? incoming) => fields.Add(new(label, existing is null ? "(no booking)" : Display(current), Display(incoming)));
        var guest = existing is null ? null : await (from link in db.ReservationCustomers join customer in db.Customers on link.CustomerID equals customer.CustomerID
            where link.ReservationID == existing.ReservationID && link.IsLeadGuest select customer.FullName).SingleOrDefaultAsync(ct);
        var assignedRooms = existing is null ? [] : await (from assignment in db.ReservationAccommodations join room in db.Rooms on assignment.RoomID equals room.RoomID
            where assignment.ReservationID == existing.ReservationID select room.RoomName).Distinct().ToArrayAsync(ct);
        var currentStatus = existing is null ? null : await db.BookingStatuses.Where(x => x.BookingStatusID == existing.BookingStatusID).Select(x => x.StatusName).SingleAsync(ct);
        var method = existing?.SourcePaymentMethodID is not int methodID ? null : await db.PaymentMethods.Where(x => x.PaymentMethodID == methodID).Select(x => x.MethodName).SingleAsync(ct);
        var nightlyCharges = existing is null ? [] : await db.ReservationCharges.AsNoTracking()
            .Where(x => x.ReservationID == existing.ReservationID && x.ChargeType == "Accommodation").OrderBy(x => x.ServiceDate).ToArrayAsync(ct);
        var incomingRoom = row.RoomID is not int roomID ? "Unresolved" : await db.Rooms.Where(x => x.RoomID == roomID).Select(x => x.RoomName).SingleAsync(ct);
        Add("Booking", existing?.ExternalReservationNumber, row.SourceNumber);
        Add("OTA", row.OTAName, row.OTAName);
        Add("Guest", guest, row.SourceGuestName);
        Add("Booked", existing?.BookedAt, source is null ? Raw("予約日時") : source.BookedAt);
        Add("Arrival", existing?.CheckInDate, source is null ? Raw("チェックイン日") : source.CheckIn);
        Add("Departure", existing?.CheckOutDate, source is null ? Raw("チェックアウト日") : source.CheckOut);
        Add("Guests", existing?.ExpectedGuestCount, source is null ? $"Adults: {Raw("大人人数")}; children: {Raw("子供人数")}" : source.GuestCount);
        Add("Status", currentStatus, source is null ? Raw("予約区分") : source.IsCancelled ? "Cancelled" : "Confirmed");
        Add("Physical room", string.Join(", ", assignedRooms), incomingRoom);
        Add("CSV room name", null, Raw("部屋名称"));
        Add("CSV room master", null, Raw("部屋マスタ"));
        Add("Payment method", method, source?.SourcePaymentMethod);
        Add("Accommodation JPY", nightlyCharges.Sum(x => x.Amount), source is null || source.IsCancelled
            ? $"Source total (unverified): {Display(Raw("合計料金"))}" : source.Nights.Sum(x => x.RoomPrice));
        foreach (var date in nightlyCharges.Where(x => x.ServiceDate is not null).Select(x => x.ServiceDate!.Value)
            .Union(source?.Nights.Select(x => x.ServiceDate) ?? []).Order())
        {
            var current = nightlyCharges.Where(x => x.ServiceDate == date).ToArray();
            var incoming = source?.Nights.Where(x => x.ServiceDate == date).ToArray();
            Add($"Night {date:yyyy-MM-dd} JPY", current.Length == 0 ? null : current.Sum(x => x.Amount), incoming is null || incoming.Length == 0 ? null : incoming.Sum(x => x.RoomPrice));
        }
        return (existing is not null, fields.AsReadOnly());
    }

    private async Task<PreparedRow> ClassifyExistingAsync(GajyunDbContext db, PreparedRow row, int otaID, CancellationToken cancellationToken)
    {
        if (row.Reservation is not ParsedReservation source) return row;
        var existing = await db.Reservations.AsNoTracking().SingleOrDefaultAsync(x => x.OTAID == otaID && x.ExternalReservationNumber == source.ExternalNumber, cancellationToken);
        if (existing is null) return row with { ExistingID = null, ExistingSnapshot = null,
            Disposition = row.NeedsRoomChoice ? ImportRowDisposition.Held : source.IsCancelled ? ImportRowDisposition.Cancelled : ImportRowDisposition.Ready };
        var latest = await db.ImportRows.AsNoTracking().Where(x => x.ReservationID == existing.ReservationID && x.ParseStatus == "Processed")
            .OrderByDescending(x => x.ImportRowID).FirstOrDefaultAsync(cancellationToken);
        row = row with { ExistingID = existing.ReservationID, ExistingSnapshot = await SnapshotAsync(db, existing, cancellationToken) };
        if (latest?.SourceRowHash == row.Fingerprint)
            return row with { Disposition = ImportRowDisposition.Duplicate, NeedsRoomChoice = false, Message = "Already imported; no changes." };
        var previous = latest is null ? null : ParseAuditedSource(latest.RawPayload);
        if (previous is null)
            return row with { Disposition = ImportRowDisposition.Held, NeedsRoomChoice = false, Message = "Previous source is unavailable. Reconcile this existing booking manually." };
        var differences = new List<string>();
        void Change(string label, object? before, object? after)
        {
            if (!Equals(before, after)) differences.Add($"{label}: {before} → {after}");
        }
        Change("Arrival", existing.CheckInDate, source.CheckIn); Change("Departure", existing.CheckOutDate, source.CheckOut);
        Change("Guests", existing.ExpectedGuestCount, (short?)source.GuestCount);
        Change("Booked", existing.BookedAt, (DateTime?)source.BookedAt);
        Change("Source modified", existing.ModifiedAtSource, source.ModifiedAtSource);
        Change("Modified flag", existing.IsModified, source.IsModified);
        Change("Guest", previous.GuestName, source.GuestName);
        Change("Room", previous.RoomMaster, source.RoomMaster);
        Change("Room name", previous.ExternalRoomName, source.ExternalRoomName);
        var oldTotal = previous.Nights.Sum(x => x.RoomPrice); var newTotal = source.Nights.Sum(x => x.RoomPrice);
        Change("Accommodation JPY", oldTotal, newTotal);
        foreach (var date in previous.Nights.Select(x => x.ServiceDate).Union(source.Nights.Select(x => x.ServiceDate)).Order())
            Change($"Night {date:yyyy-MM-dd}", previous.Nights.FirstOrDefault(x => x.ServiceDate == date)?.RoomPrice, source.Nights.FirstOrDefault(x => x.ServiceDate == date)?.RoomPrice);
        Change("Payment method", previous.SourcePaymentMethod, source.SourcePaymentMethod);
        var cancelledStatus = await db.BookingStatuses.Where(x => x.StatusName == "Cancelled").Select(x => x.BookingStatusID).SingleAsync(cancellationToken);
        Change("Status", existing.BookingStatusID == cancelledStatus ? "Cancelled" : "Active", source.IsCancelled ? "Cancelled" : "Active");
        if (previous.Details != source.Details) differences.Add("Details text changed; both source versions remain in import history.");
        var message = differences.Count == 0 ? "Other source fields changed (original versions retained in audit)." : string.Join("; ", differences);
        var previousRoom = ResolveRoom(previous, otaID, await LoadMappingsAsync(db, cancellationToken));
        var unsafeReason = await UnsafeUpdateReasonAsync(db, existing, previous, source, previousRoom.RoomID, cancellationToken);
        if (unsafeReason is not null) return row with { Disposition = ImportRowDisposition.Held, NeedsRoomChoice = false, Message = message + ". " + unsafeReason };
        return row with { Disposition = source.IsCancelled && existing.BookingStatusID != cancelledStatus ? ImportRowDisposition.CancellationUpdate : ImportRowDisposition.Changed,
            Message = (source.IsCancelled && existing.BookingStatusID != cancelledStatus ? "Previously active → CANCELLED. " : "Changed booking. ") + message + ". Apply with batch approval; staff data stays unchanged." };
    }

    private ParsedReservation? ParseAuditedSource(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("Headers", out var headers) || !document.RootElement.TryGetProperty("Values", out var values) ||
                headers.ValueKind != JsonValueKind.Array || values.ValueKind != JsonValueKind.Array ||
                headers.EnumerateArray().Any(x => x.ValueKind != JsonValueKind.String) || values.EnumerateArray().Any(x => x.ValueKind != JsonValueKind.String)) return null;
            var names = headers.EnumerateArray().Select(x => x.GetString()!).ToArray();
            var fields = values.EnumerateArray().Select(x => x.GetString()!).ToArray();
            if (names.Length != fields.Length || names.Distinct(StringComparer.Ordinal).Count() != names.Length) return null;
            return parser.Parse(names.Select((name, index) => new KeyValuePair<string, string>(name, fields[index])).ToDictionary()).Reservation;
        }
        catch (JsonException)
        {
            logger.LogError("Stored import audit contains invalid JSON. Source records were not logged.");
            throw new ReservationImportException("Previous import history is damaged. No changes were saved; reconcile the stored source audit before importing again.");
        }
    }

    private static async Task<string?> UnsafeUpdateReasonAsync(GajyunDbContext db, Reservation existing, ParsedReservation previous, ParsedReservation source, int? previousRoomID, CancellationToken ct)
    {
        if (RoomNameNormalizer.Normalize(previous.GuestName) != RoomNameNormalizer.Normalize(source.GuestName)) return "Guest identity changed; staff reconciliation is required.";
        var previousStatusID = await db.BookingStatuses.Where(x => x.StatusName == (previous.IsCancelled ? "Cancelled" : "Confirmed")).Select(x => x.BookingStatusID).SingleAsync(ct);
        if (existing.BookingStatusID != previousStatusID || existing.BookingDetails != previous.Details || existing.RoomRateInfo != previous.RoomRateInfo ||
            existing.ConsecutiveStayInfo != previous.ConsecutiveInfo || existing.IsModified != previous.IsModified || existing.ModifiedAtSource != previous.ModifiedAtSource)
            return "Staff changed booking status or source text; automatic overwrite is blocked.";
        if (existing.CheckInDate != previous.CheckIn || existing.CheckOutDate != previous.CheckOut || existing.ExpectedGuestCount != previous.GuestCount || existing.BookedAt != previous.BookedAt)
            return "Staff changed booking fields; automatic overwrite is blocked.";
        var previousPaymentMethodID = previous.SourcePaymentMethod is null ? (int?)null :
            await db.PaymentMethods.Where(x => x.MethodName == previous.SourcePaymentMethod).Select(x => (int?)x.PaymentMethodID).SingleOrDefaultAsync(ct);
        if (existing.SourcePaymentMethodID != previousPaymentMethodID)
            return "Staff changed the source payment method; automatic overwrite is blocked.";
        var dateChange = existing.CheckInDate != source.CheckIn || existing.CheckOutDate != source.CheckOut;
        if (dateChange && existing.ActualCheckInAt is not null) return "An actual check-in exists; date changes require staff reconciliation.";
        var charges = await db.ReservationCharges.AsNoTracking().Where(x => x.ReservationID == existing.ReservationID && x.SourceLineKey != null).ToArrayAsync(ct);
        if (charges.Any(x => !x.SourceLineKey!.StartsWith("combined-jp-", StringComparison.Ordinal) || x.ChargeType != "Accommodation") ||
            charges.Length != previous.Nights.Count || charges.Any(x => x.Quantity != 1m || !previous.Nights.Any(n => n.ServiceDate == x.ServiceDate && n.RoomPrice == x.UnitPrice && n.Description == x.Description)))
            return "Imported nightly charges were edited; staff reconciliation is required.";
        var chargeIDs = charges.Select(x => x.ChargeID).ToArray();
        if (!source.IsCancelled && await db.PaymentAllocations.AnyAsync(x => chargeIDs.Contains(x.ChargeID), ct)) return "Payments are allocated to nightly charges; financial changes require reconciliation.";
        var assignments = await db.ReservationAccommodations.AsNoTracking().Where(x => x.ReservationID == existing.ReservationID).ToArrayAsync(ct);
        if (!source.IsCancelled && (assignments.Length != (previous.IsCancelled ? 0 : 1) || assignments.Any(x => x.BedID != null ||
                x.RoomID != previousRoomID || x.StartDate != previous.CheckIn || x.EndDate != previous.CheckOut)))
            return "Accommodation assignments were edited; staff reconciliation is required.";
        var stays = await db.StayReservations.Where(x => x.ReservationID == existing.ReservationID).Select(x => x.StayID).ToArrayAsync(ct);
        if (dateChange && await db.StayReservations.AnyAsync(x => stays.Contains(x.StayID) && x.ReservationID != existing.ReservationID, ct))
            return "Dates changed on a multi-booking Stay; staff reconciliation is required.";
        if (!source.IsCancelled && await db.ReservationTaxes.AnyAsync(x => x.ReservationID == existing.ReservationID &&
                (x.CalculationStatus != "PendingReview" || x.TaxablePricePerPerson != null || x.LegalTaxAmount != null || x.ChargedTaxAmount != null), ct))
            return "Historical taxes are confirmed; financial changes require reconciliation.";
        var statements = await db.OTAStatementItems.AsNoTracking().Where(x => x.ReservationID == existing.ReservationID && x.SourceLineKey != null).ToArrayAsync(ct);
        if (!source.IsCancelled && (statements.Length != previous.Statements.Count || statements.Any(x => x.Classification != "Unclassified" ||
                !previous.Statements.Any(y => y.Label == x.RawLabel && y.Amount == x.Amount && y.RawText == x.RawText))))
            return "OTA statements were edited or classified by staff; financial changes require reconciliation.";
        return null;
    }

    private static async Task<string> SnapshotAsync(GajyunDbContext db, Reservation reservation, CancellationToken ct) => JsonSerializer.Serialize(new
    {
        reservation,
        sourceVersion = await db.ImportRows.AsNoTracking().Where(x => x.ReservationID == reservation.ReservationID && x.ParseStatus == "Processed")
            .OrderByDescending(x => x.ImportRowID).Select(x => new { x.ImportRowID, x.SourceRowHash }).FirstOrDefaultAsync(ct),
        charges = await db.ReservationCharges.AsNoTracking().Where(x => x.ReservationID == reservation.ReservationID).OrderBy(x => x.ChargeID).ToArrayAsync(ct),
        assignments = await db.ReservationAccommodations.AsNoTracking().Where(x => x.ReservationID == reservation.ReservationID).OrderBy(x => x.ReservationAccommodationID).ToArrayAsync(ct),
        taxes = await db.ReservationTaxes.AsNoTracking().Where(x => x.ReservationID == reservation.ReservationID).OrderBy(x => x.ReservationTaxID).ToArrayAsync(ct),
        guests = await db.ReservationCustomers.AsNoTracking().Where(x => x.ReservationID == reservation.ReservationID).OrderBy(x => x.CustomerID).ToArrayAsync(ct)
    });

    private RoomMatchResult ResolveRoom(ParsedReservation source, int? otaID, IReadOnlyCollection<OTARoomMapping> mappings)
    {
        var external = roomMapper.Match(otaID, source.ExternalRoomName, mappings);
        var master = roomMapper.Match(null, source.RoomMaster, mappings);
        if (external.Status == RoomMatchStatus.Ambiguous || master.Status == RoomMatchStatus.Ambiguous)
            return new(RoomMatchStatus.Ambiguous, null, "The room alias or source room master is ambiguous. No physical room will be created.");
        if (external.Status == RoomMatchStatus.Matched && master.Status == RoomMatchStatus.Matched && external.RoomID != master.RoomID)
            return new(RoomMatchStatus.Ambiguous, null, "The external room and source room master point to different physical rooms. Staff review is required.");
        return external.Status == RoomMatchStatus.Matched ? external : master.Status == RoomMatchStatus.Matched
            ? new(RoomMatchStatus.Matched, master.RoomID, "Matched the export's room master to a known physical room alias.") : external;
    }

    private static async Task<long> ResolveCustomerAsync(GajyunDbContext db, ParsedReservation source,
        IDictionary<string, long> customersByName, CancellationToken ct)
    {
        var name = RoomNameNormalizer.Normalize(source.GuestName);
        if (name.Length == 0) throw new ReservationImportException("A guest name is required to resolve the customer; no data was saved.");
        if (customersByName.TryGetValue(name, out var customerID)) return customerID;
        var created = new Customer { FullName = source.GuestName };
        db.Customers.Add(created);
        await db.SaveChangesAsync(ct);
        customersByName.Add(name, created.CustomerID);
        return created.CustomerID;
    }

    private static async Task SaveSourceMappingAsync(GajyunDbContext db, int otaID, string externalName, int roomID, bool explicitlyChosen, CancellationToken ct)
    {
        var normalized = RoomNameNormalizer.Normalize(externalName);
        if (normalized.Length == 0) throw new ReservationImportException("An external room name is required to save its OTA mapping.");
        var mapping = await db.OTARoomMappings.SingleOrDefaultAsync(x => x.OTAID == otaID && x.NormalizedName == normalized && x.IsActive, ct);
        if (mapping is null) db.OTARoomMappings.Add(new OTARoomMapping { OTAID = otaID, RoomID = roomID, ExternalRoomName = externalName, NormalizedName = normalized });
        else if (mapping.RoomID != roomID)
        {
            if (!explicitlyChosen) throw new ReservationImportException("Conflicting OTA room mapping. Choose which physical room is correct.");
            mapping.RoomID = roomID;
        }
        await db.SaveChangesAsync(ct);
    }

    private static string? GroupKey(ParsedReservation source, int otaID)
    {
        if (source.OTAName is not ("Booking.com" or "Hostelworld")) return null;
        var separator = source.ExternalNumber.LastIndexOf('-');
        if (separator <= 0 || separator == source.ExternalNumber.Length - 1 || !source.ExternalNumber[(separator + 1)..].All(char.IsAsciiDigit)) return null;
        var key = $"{otaID}:{source.ExternalNumber[..separator]}";
        if (key.Length > 120) throw new ReservationImportException("The booking group reference exceeds the schema limit.");
        return key;
    }

    private static async Task<Dictionary<string, long?>> PrepareGroupsAsync(GajyunDbContext db, IReadOnlyList<PreparedRow> rows,
        IReadOnlyDictionary<string, OTA> otas, CancellationToken ct)
    {
        var identities = rows.Where(x => x.Reservation is not null && !x.NeedsRoomChoice &&
            x.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled or ImportRowDisposition.Changed or ImportRowDisposition.CancellationUpdate or ImportRowDisposition.Duplicate)
            .Select(x => new { Key = GroupKey(x.Reservation!, otas[x.OTAName].OTAID), x.Reservation!.ExternalNumber }).Where(x => x.Key is not null).ToList();
        var existing = await (from booking in db.Reservations.AsNoTracking() join ota in db.OTAs on booking.OTAID equals ota.OTAID
            where ota.OTAName == "Booking.com" || ota.OTAName == "Hostelworld"
            select new { booking.OTAID, ota.OTAName, booking.ExternalReservationNumber }).ToArrayAsync(ct);
        foreach (var booking in existing)
        {
            if (booking.ExternalReservationNumber is not string number) continue;
            var separator = number.LastIndexOf('-');
            if (separator > 0 && separator < number.Length - 1 && number[(separator + 1)..].All(char.IsAsciiDigit))
                identities.Add(new { Key = (string?)$"{booking.OTAID}:{number[..separator]}", ExternalNumber = number });
        }
        var result = new Dictionary<string, long?>(StringComparer.Ordinal);
        foreach (var group in identities.GroupBy(x => x.Key!).Where(x => x.Select(y => y.ExternalNumber).Distinct().Count() > 1))
        {
            var record = await db.ReservationGroups.SingleOrDefaultAsync(x => x.ExternalGroupReference == group.Key, ct);
            if (record is null)
            {
                record = new ReservationGroup { ExternalGroupReference = group.Key, LogoUrl = Seeding.LookupSeedData.CombinedLogoUrl, Notes = "Automatic OTA-scoped booking-number prefix group." };
                db.ReservationGroups.Add(record); await db.SaveChangesAsync(ct);
            }
            result.Add(group.Key, record.GroupID);
        }
        return result;
    }

    private static async Task AssignExistingGroupsAsync(GajyunDbContext db, IReadOnlyDictionary<string, long?> groups, CancellationToken ct)
    {
        foreach (var group in groups)
        {
            var separator = group.Key.IndexOf(':');
            var otaID = int.Parse(group.Key[..separator], System.Globalization.CultureInfo.InvariantCulture);
            var prefix = group.Key[(separator + 1)..];
            var candidates = await db.Reservations.Where(x => x.OTAID == otaID && x.GroupID == null && x.ExternalReservationNumber != null &&
                x.ExternalReservationNumber.StartsWith(prefix + "-")).ToArrayAsync(ct);
            foreach (var member in candidates)
            {
                var suffix = member.ExternalReservationNumber![(prefix.Length + 1)..];
                if (suffix.Length > 0 && suffix.All(char.IsAsciiDigit)) member.GroupID = group.Value;
            }
        }
    }

    private static async Task LinkStayAsync(GajyunDbContext db, Reservation reservation, long customerID, CancellationToken ct)
    {
        var existingLinks = await db.StayReservations.Where(x => x.ReservationID == reservation.ReservationID).Select(x => x.StayID).ToArrayAsync(ct);
        if (existingLinks.Length > 0) return;
        var cancelledStatusID = await db.BookingStatuses.Where(x => x.StatusName == "Cancelled").Select(x => x.BookingStatusID).SingleAsync(ct);
        var adjacent = await db.Stays.Where(x => x.CustomerID == customerID &&
            (x.EndDate == reservation.CheckInDate || x.StartDate == reservation.CheckOutDate) &&
            !db.StayReservations.Any(link => link.StayID == x.StayID && db.Reservations.Any(booking =>
                booking.ReservationID == link.ReservationID && booking.BookingStatusID == cancelledStatusID))).OrderBy(x => x.StartDate).ToArrayAsync(ct);
        var stay = adjacent.FirstOrDefault() ?? new Stay { CustomerID = customerID, StartDate = reservation.CheckInDate, EndDate = reservation.CheckOutDate };
        if (stay.StayID == 0) { db.Stays.Add(stay); await db.SaveChangesAsync(ct); }
        stay.StartDate = stay.StartDate < reservation.CheckInDate ? stay.StartDate : reservation.CheckInDate;
        stay.EndDate = stay.EndDate > reservation.CheckOutDate ? stay.EndDate : reservation.CheckOutDate;
        foreach (var other in adjacent.Skip(1))
        {
            var links = await db.StayReservations.Where(x => x.StayID == other.StayID).ToArrayAsync(ct);
            foreach (var link in links)
            {
                db.StayReservations.Remove(link);
                if (!await db.StayReservations.AnyAsync(x => x.StayID == stay.StayID && x.ReservationID == link.ReservationID, ct))
                    db.StayReservations.Add(new StayReservation { StayID = stay.StayID, ReservationID = link.ReservationID, AddedAt = link.AddedAt });
            }
            foreach (var memo in await db.StayMemos.Where(x => x.StayID == other.StayID).ToArrayAsync(ct)) memo.StayID = stay.StayID;
            stay.StartDate = stay.StartDate < other.StartDate ? stay.StartDate : other.StartDate;
            stay.EndDate = stay.EndDate > other.EndDate ? stay.EndDate : other.EndDate;
            await db.SaveChangesAsync(ct); db.Stays.Remove(other);
        }
        db.StayReservations.Add(new StayReservation { StayID = stay.StayID, ReservationID = reservation.ReservationID });
        await db.SaveChangesAsync(ct);
    }

    private static async Task ApplyUpdateAsync(GajyunDbContext db, Reservation existing, ParsedReservation source, int roomID,
        int statusID, int? paymentMethodID, int? appliedPolicyID, TaxPolicy[] policies, CancellationToken ct)
    {
        existing.BookingStatusID = statusID; existing.BookedAt = source.BookedAt; existing.ModifiedAtSource = source.ModifiedAtSource;
        existing.CheckInDate = source.CheckIn; existing.CheckOutDate = source.CheckOut; existing.ExpectedGuestCount = source.GuestCount;
        existing.IsModified = source.IsModified; existing.ConsecutiveStayInfo = source.ConsecutiveInfo;
        existing.RoomRateInfo = source.RoomRateInfo; existing.BookingDetails = source.Details; existing.SourcePaymentMethodID = paymentMethodID;
        if (source.IsCancelled) return;
        var accommodation = await db.ReservationAccommodations.Where(x => x.ReservationID == existing.ReservationID).ToArrayAsync(ct);
        if (accommodation.Length == 0) db.ReservationAccommodations.Add(new ReservationAccommodation
            { ReservationID = existing.ReservationID, RoomID = roomID, StartDate = source.CheckIn, EndDate = source.CheckOut });
        foreach (var assignment in accommodation) { assignment.RoomID = roomID; assignment.StartDate = source.CheckIn; assignment.EndDate = source.CheckOut; }
        var oldCharges = await db.ReservationCharges.Where(x => x.ReservationID == existing.ReservationID && x.SourceLineKey != null).ToArrayAsync(ct);
        db.ReservationCharges.RemoveRange(oldCharges);
        var oldStatements = await db.OTAStatementItems.Where(x => x.ReservationID == existing.ReservationID && x.SourceLineKey != null).ToArrayAsync(ct);
        db.OTAStatementItems.RemoveRange(oldStatements);
        await db.SaveChangesAsync(ct);
        foreach (var night in source.Nights) db.ReservationCharges.Add(new ReservationCharge
        {
            ReservationID = existing.ReservationID, ChargeType = "Accommodation", ServiceDate = night.ServiceDate, Description = night.Description,
            Quantity = 1m, UnitPrice = night.RoomPrice, SourceLineKey = $"{ParserVersion}:night:{night.ServiceDate:yyyy-MM-dd}"
        });
        for (var i = 0; i < source.Statements.Count; i++) db.OTAStatementItems.Add(new OTAStatementItem
        {
            ReservationID = existing.ReservationID, RawLabel = source.Statements[i].Label, RawText = source.Statements[i].RawText,
            Amount = source.Statements[i].Amount, Classification = "Unclassified", SourceLineKey = $"{ParserVersion}:statement:{i}"
        });
        var oldTaxes = await db.ReservationTaxes.Where(x => x.ReservationID == existing.ReservationID).ToArrayAsync(ct);
        db.ReservationTaxes.RemoveRange(oldTaxes.Where(x => !source.Nights.Any(n => n.ServiceDate == x.ServiceDate)));
        foreach (var night in source.Nights.Where(n => !oldTaxes.Any(x => x.ServiceDate == n.ServiceDate)))
        {
            var legal = policies.Where(x => x.ValidFrom <= night.ServiceDate && (x.ValidTo == null || night.ServiceDate < x.ValidTo)).ToArray();
            db.ReservationTaxes.Add(new ReservationTax { ReservationID = existing.ReservationID, RoomID = roomID, ServiceDate = night.ServiceDate,
                GuestCount = source.GuestCount, AppliedTaxPolicyID = appliedPolicyID, LegalTaxPolicyID = legal.Length == 1 ? legal[0].TaxPolicyID : null,
                CalculationStatus = "PendingReview", Notes = "Breakfast-inclusive source price; taxable base not established." });
        }
        var customerID = await db.ReservationCustomers.Where(x => x.ReservationID == existing.ReservationID && x.IsLeadGuest).Select(x => x.CustomerID).SingleAsync(ct);
        await LinkStayAsync(db, existing, customerID, ct);
        var stayIDs = await db.StayReservations.Where(x => x.ReservationID == existing.ReservationID).Select(x => x.StayID).ToArrayAsync(ct);
        foreach (var stay in await db.Stays.Where(x => stayIDs.Contains(x.StayID)).ToArrayAsync(ct))
            if (await db.StayReservations.CountAsync(x => x.StayID == stay.StayID, ct) == 1) { stay.StartDate = source.CheckIn; stay.EndDate = source.CheckOut; }
    }

    private static Task<OTARoomMapping[]> LoadMappingsAsync(GajyunDbContext db, CancellationToken cancellationToken) =>
        db.OTARoomMappings.AsNoTracking().Where(x => x.IsActive && db.Rooms.Any(room => room.RoomID == x.RoomID && room.IsActive)).ToArrayAsync(cancellationToken);

    private ReservationImportException DatabaseError(int number)
    {
        logger.LogError("Reservation import database operation failed with SQL error {Number}. Source records were not logged.", number);
        return new ReservationImportException($"The import could not access SQL Server (error {number}). Check database initialization and validate again to confirm persisted outcomes before retrying.");
    }

    private sealed record PreparedRow(int Number, string OTAName, string RawPayload, string Fingerprint, string RawDetails,
        ParsedReservation? Reservation, int? RoomID, ImportRowDisposition Disposition, string Message)
    {
        public long? ExistingID { get; init; }
        public string? ExistingSnapshot { get; init; }
        public bool NeedsRoomChoice { get; init; }
        public string SourceNumber { get; init; } = "";
        public string SourceGuestName { get; init; } = "";
        public DateTime? SourceBookedAt { get; init; }
        public IReadOnlyDictionary<string, string> SourceFields { get; init; } = new Dictionary<string, string>();
    }
    private sealed record PreparedImport(Guid ID, CsvImportFile File, IReadOnlyList<PreparedRow> Rows, IReadOnlyList<ImportReviewRow> ReviewRows);
}

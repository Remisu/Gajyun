using System.IO;
using System.Text;
using Gajyun.Core.Csv;
using Gajyun.Core.Dictionary;
using Gajyun.Core.Models;
using Gajyun.Core.Services;
using Gajyun.Data;
using Gajyun.Desktop.Services;
using Gajyun.Desktop.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
    checks++;
}
async Task Reject(Func<Task> action, string message)
{
    try { await action(); }
    catch (ReservationImportException) { Check(true, message); return; }
    throw new InvalidOperationException("FAIL: " + message);
}
const string server = "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;";
var testName = "Gajyun_Phase5_Verification_" + Guid.NewGuid().ToString("N");
var directory = Path.Combine(AppContext.BaseDirectory, "Fixtures_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
var files = new List<string>();
var databaseAttempted = false;
var sourcePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "docs", "20261106154030.csv"));
Dictionary<string, string> Fields(string id, string status = "予約")
{
    var result = CombinedExportReservationParser.RequiredHeaders.ToDictionary(x => x, _ => "");
    result["予約サイト名"] = "Hostelworld"; result["予約番号"] = id;
    result["予約日時"] = "2026-10-01 10:11:12"; result["予約区分"] = status;
    result["チェックイン日"] = "2026-10-20"; result["チェックアウト日"] = "2026-10-23";
    result["宿泊者氏名"] = "Synthetic Verification Guest"; result["部屋名称"] = "翔(Show)";
    result["部屋マスタ"] = "翔(Show)"; result["部屋数"] = "2"; result["大人人数"] = "2";
    result["子供人数"] = "0"; result["合計料金"] = "17100"; result["泊数"] = "3";
    result["詳細"] = "[2026-10-20] breakfast non-refundable JPY 5700.00 2人[2026-10-21] breakfast non-refundable JPY 5700.00 2人[2026-10-22] breakfast non-refundable JPY 5700.00 2人deposit JPY 2052.00mixed:1";
    result["未確認項目"] = "unknown,quoted \"value\"\r\n日本語";
    return result;
}
async Task<string> Fixture(string name, params Dictionary<string, string>[] rows)
{
    var headers = rows[0].Keys.ToArray();
    string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    var text = string.Join(',', headers.Select(Quote)) + "\r\n" + string.Join("\r\n", rows.Select(row => string.Join(',', headers.Select(header => Quote(row.GetValueOrDefault(header, "")))))) + "\r\n";
    var path = Path.Combine(directory, name + ".csv");
    await File.WriteAllTextAsync(path, text, new UTF8Encoding(true, true)); files.Add(path); return path;
}
try
{
    var services = new ServiceCollection().AddLogging().AddGajyunData(server + "Database=" + testName + ";").BuildServiceProvider();
    await using var provider = services;
    var reader = provider.GetRequiredService<ICsvImportFileReader>();
    var parser = provider.GetRequiredService<ICsvReservationParser>();
    var detailParser = provider.GetRequiredService<NightlyDetailsParser>();
    var sample = await reader.ReadForImportAsync(sourcePath, CsvFileEncoding.Auto, ',');
    var originalBytes = await File.ReadAllBytesAsync(sourcePath);
    Check(sample.Content.Headers.Length == 51 && sample.Content.TotalRows == 340 && sample.Content.Records.Count == 340 && !sample.Content.IsTruncated, "real sample imports all 340 records, not only preview rows");
    Check(sample.Content.EncodingDescription.Contains("CP932") && sample.OriginalBytes.SequenceEqual(originalBytes), "CP932 source retained byte for byte");
    var nightly = detailParser.Parse(Fields("UNIT")["詳細"]);
    Check(nightly.Nights.Count == 3 && nightly.Nights.All(x => x.RoomPrice == 5700m && x.ReportedGuestCount == 2) && nightly.Nights.Sum(x => x.RoomPrice) == 17100m, "concatenated nightly totals never multiplied by guests or units");
    Check(nightly.Statements.Count == 1 && nightly.Statements[0].Label == "deposit" && nightly.Statements[0].Amount == 2052m, "ambiguous deposit parsed separately from accommodation");
    Check(parser.Parse(Fields("UNIT")).Succeeded, "synthetic verified source accepts quoted optional unknown fields");
    var agodaFields = Fields("AGODA"); agodaFields["予約サイト名"] = "Agoda";
    Check(!parser.Parse(agodaFields).Succeeded, "Agoda held even with superficially parseable prices");
    var cancelledFields = Fields("CANCEL", "キャンセル"); cancelledFields["詳細"] = "unverified historical price";
    Check(parser.Parse(cancelledFields).Reservation is { IsCancelled: true, Nights.Count: 0, Statements.Count: 0 }, "cancelled source bypasses all financial parsing");
    var gapFields = Fields("GAP"); gapFields["詳細"] = "[2026-10-20] JPY 17100.00 2人";
    Check(!parser.Parse(gapFields).Succeeded, "missing nights rejected despite matching accommodation total");
    var mismatch = Fields("MISMATCH"); mismatch["合計料金"] = "34200";
    Check(!parser.Parse(mismatch).Succeeded, "guest-multiplied aggregate cannot silently replace nightly room prices");
    var precision = Fields("PRECISION"); precision["詳細"] = precision["詳細"].Replace("5700.00", "5700.001");
    Check(!parser.Parse(precision).Succeeded, "three-decimal nightly price cannot be silently truncated");
    var grouping = Fields("GROUPING"); grouping["合計料金"] = "17,10,0";
    Check(!parser.Parse(grouping).Succeeded, "malformed thousands grouping is rejected");
    var negative = Fields("NEGATIVE"); negative["詳細"] = negative["詳細"].Replace("5700.00", "-5700.00");
    Check(!parser.Parse(negative).Succeeded, "negative nightly price never consumes a later deposit as the room price");
    var dict = provider.GetRequiredService<IDataDictionaryService>().Load();
    Check(dict.SchemaVersion == "1.2" && dict.Tables.Count == 28 && dict.Tables.Single(x => x.Name == "ImportBatch").Fields.Single(x => x.Name == "SourceFileContent").Version == "1.2", "schema v1.2 dictionary available offline with complete audit metadata");
    databaseAttempted = true;
    var init = await provider.GetRequiredService<IDatabaseInitializationService>().InitializeAsync();
    Check(init.Succeeded, "actual LocalDB connection creates isolated verification database through migrations");
    var factory = provider.GetRequiredService<IDbContextFactory<GajyunDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    Check((await db.Database.GetAppliedMigrationsAsync()).Count() == 2 && !(await db.Database.GetPendingMigrationsAsync()).Any(), "both baseline and additive audit migration applied");
    var importer = provider.GetRequiredService<IReservationImportService>();
    var review = await importer.PrepareAsync(sourcePath, CsvFileEncoding.Auto, ',');
    Console.WriteLine($"SAMPLE REVIEW: total={review.TotalRows}; ready={review.ReadyRows}; cancelled={review.CancelledRows}; held={review.HeldRows}; invalid={review.InvalidRows}");
    foreach (var group in review.Rows.Where(x => x.Disposition is ImportRowDisposition.Held or ImportRowDisposition.Invalid).GroupBy(x => x.Message)) Console.WriteLine($"REVIEW REASON: {group.Count()} rows: {group.Key}");
    Check(review.TotalRows == 340 && review.InvalidRows == 0 && review.Rows.Count(x => x.OTAName == "Agoda" && x.Disposition == ImportRowDisposition.Held) == 7, "all seven Agoda rows held including cancelled Agoda records");
    Check(review.CancelledRows == 74 && review.ReadyRows == 257 && review.HeldRows == 9 && review.DuplicateRows == 0, "exact sample outcomes: 257 active, 74 cancelled, seven Agoda holds and two conflicting nightly schedules");
    var expectedCount = review.ReadyRows + review.CancelledRows;
    var expectedCharges = review.Rows.Where(x => x.Disposition == ImportRowDisposition.Ready).Sum(x => x.NightCount);
    var expectedMoney = review.Rows.Where(x => x.Disposition == ImportRowDisposition.Ready).Sum(x => x.AccommodationTotal);
    var result = await importer.ImportAsync(review.ReviewID);
    Check(result.CreatedReservations == expectedCount && result.CreatedCharges == expectedCharges && result.BatchesRecorded == 4, "real sample imported in one transaction with per-OTA audit batches");
    Check(await db.Reservations.CountAsync() == expectedCount && await db.ReservationCharges.CountAsync() == expectedCharges && await db.ReservationCharges.SumAsync(x => x.Amount) == expectedMoney, "persisted nightly amounts exactly match reviewed total-room prices");
    Check(await db.ImportRows.CountAsync() == 340 && (await db.ImportBatches.ToArrayAsync()).All(x => x.SourceFileContent!.SequenceEqual(sample.OriginalBytes) && x.SourceFileHash!.Length == 64 && x.SourceEncoding!.Contains("CP932")), "every row audited and every batch preserves exact original source bytes");
    var cancelID = await db.BookingStatuses.Where(x => x.StatusName == "Cancelled").Select(x => x.BookingStatusID).SingleAsync();
    var cancelledIDs = db.Reservations.Where(x => x.BookingStatusID == cancelID).Select(x => x.ReservationID);
    Check(await cancelledIDs.CountAsync() == 74 && !await db.ReservationCharges.AnyAsync(x => cancelledIDs.Contains(x.ReservationID)) && !await db.OTAStatementItems.AnyAsync(x => cancelledIDs.Contains(x.ReservationID)) && !await db.ReservationTaxes.AnyAsync(x => cancelledIDs.Contains(x.ReservationID)) && !await db.ReservationAccommodations.AnyAsync(x => cancelledIDs.Contains(x.ReservationID)), "cancelled bookings create no charges, statements, tax or accommodation assignments");
    Check(await db.ReservationCustomers.CountAsync() == expectedCount && await db.Customers.CountAsync() == expectedCount, "one separate customer association per original reservation; no name-only identity merge");
    Check(await db.Rooms.CountAsync() == 5 && await db.Beds.CountAsync() == 14 && !await db.ReservationAccommodations.AnyAsync(x => x.BedID != null), "physical rooms and beds unchanged; import never guesses bed assignments");
    var privateRooms = db.Rooms.Where(x => x.Capacity == 2).Select(x => x.RoomID);
    Check(await db.ReservationAccommodations.AnyAsync(x => privateRooms.Contains(x.RoomID) && x.BedID == null), "private rooms with no individual beds accept room-level bookings");
    Check(!await db.ReservationPayments.AnyAsync() && !await db.ReservationCharges.AnyAsync(x => x.ChargeType != "Accommodation") && await db.OTAStatementItems.AllAsync(x => x.Classification == "Unclassified"), "source deposits and commission labels never become receipts, revenue or extra charges");
    Check(await db.ReservationTaxes.CountAsync() == expectedCharges && await db.ReservationTaxes.AllAsync(x => x.CalculationStatus == "PendingReview" && x.TaxablePricePerPerson == null && x.LegalTaxAmount == null && x.ChargedTaxAmount == null), "breakfast-inclusive prices leave taxable base and tax amounts pending");
    var initialStatements = await db.OTAStatementItems.CountAsync();
    var repeat = await importer.PrepareAsync(sourcePath, CsvFileEncoding.Auto, ',');
    Check(repeat.DuplicateRows == expectedCount && repeat.ReadyRows == 0 && repeat.CancelledRows == 0, "same-file reimport recognizes unchanged source booking identities");
    await importer.ImportAsync(repeat.ReviewID);
    Check(await db.Reservations.CountAsync() == expectedCount && await db.ReservationCharges.CountAsync() == expectedCharges && await db.OTAStatementItems.CountAsync() == initialStatements && await db.Customers.CountAsync() == expectedCount && await db.ImportRows.CountAsync() == 680, "repeat import duplicates no business records but records full import history");
    var fixture = await Fixture("synthetic", Fields("SYNTHETIC-A"));
    var syntheticReview = await importer.PrepareAsync(fixture, CsvFileEncoding.Auto, ',');
    Check(syntheticReview.ReadyRows == 1, "synthetic two-guest, two-unit room-total booking ready");
    await importer.ImportAsync(syntheticReview.ReviewID);
    var synthetic = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "SYNTHETIC-A");
    Check(await db.ReservationCharges.Where(x => x.ReservationID == synthetic.ReservationID).SumAsync(x => x.Amount) == 17100m && await db.OTAStatementItems.Where(x => x.ReservationID == synthetic.ReservationID).Select(x => x.Amount).SingleAsync() == 2052m, "SQL amounts remain JPY 17100 accommodation plus separate unclassified 2052 statement");
    Check((await db.ImportRows.Where(x => x.ReservationID == synthetic.ReservationID).SingleAsync()).RawPayload!.Contains("mixed:1"), "unknown detail suffix retained in durable raw source payload");
    var groupRecord = new ReservationGroup { Notes = "staff group" }; db.ReservationGroups.Add(groupRecord); await db.SaveChangesAsync();
    synthetic.GroupID = groupRecord.GroupID; synthetic.ActualCheckInAt = new DateTime(2026, 10, 20, 17, 30, 0); synthetic.CheckInStatus = "CheckedIn";
    db.ReservationMemos.Add(new ReservationMemo { ReservationID = synthetic.ReservationID, MemoText = "staff note" });
    db.ReservationPayments.Add(new ReservationPayment { ReservationID = synthetic.ReservationID, Direction = "Inflow", CounterpartyType = "Guest", PaymentKind = "Payment", Amount = 123m });
    db.ReservationCharges.Add(new ReservationCharge { ReservationID = synthetic.ReservationID, ChargeType = "Towel", Description = "staff extra", UnitPrice = 100m });
    var staffCharge = await db.ReservationCharges.FirstAsync(x => x.ReservationID == synthetic.ReservationID && x.SourceLineKey != null); staffCharge.UnitPrice = 5600m;
    await db.SaveChangesAsync();
    var taxesBefore = await db.ReservationTaxes.Where(x => x.ReservationID == synthetic.ReservationID).Select(x => new { x.ReservationTaxID, x.AppliedTaxPolicyID, x.LegalTaxPolicyID }).ToArrayAsync();
    var privateRoom = await db.Rooms.SingleAsync(x => x.RoomName == "翔(Show)");
    privateRoom.DefaultTaxPolicyID = await db.TaxPolicies.OrderBy(x => x.ValidFrom).Select(x => x.TaxPolicyID).FirstAsync(); await db.SaveChangesAsync();
    var unchanged = await importer.PrepareAsync(fixture, CsvFileEncoding.Auto, ','); await importer.ImportAsync(unchanged.ReviewID);
    db.ChangeTracker.Clear(); synthetic = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "SYNTHETIC-A");
    Check(synthetic.GroupID == groupRecord.GroupID && synthetic.ActualCheckInAt == new DateTime(2026, 10, 20, 17, 30, 0) && synthetic.CheckInStatus == "CheckedIn", "reimport preserves manual check-in and group assignment");
    Check(await db.ReservationPayments.Where(x => x.ReservationID == synthetic.ReservationID).SumAsync(x => x.Amount) == 123m && await db.ReservationMemos.AnyAsync(x => x.ReservationID == synthetic.ReservationID && x.MemoText == "staff note") && await db.ReservationCharges.CountAsync(x => x.ReservationID == synthetic.ReservationID) == 4 && (await db.ReservationCharges.FindAsync(staffCharge.ChargeID))!.UnitPrice == 5600m, "reimport preserves staff payments, notes, extra charges and manually edited rates");
    Check((await db.ReservationTaxes.Where(x => x.ReservationID == synthetic.ReservationID).Select(x => new { x.ReservationTaxID, x.AppliedTaxPolicyID, x.LegalTaxPolicyID }).ToArrayAsync()).SequenceEqual(taxesBefore), "changing default room tax preference never rewrites historical tax snapshots");
    var changedFields = Fields("SYNTHETIC-A", "キャンセル");
    var changedFile = await Fixture("changed", changedFields);
    var changedReview = await importer.PrepareAsync(changedFile, CsvFileEncoding.Auto, ',');
    Check(changedReview.HeldRows == 1 && changedReview.Rows[0].Message.Contains("reconciliation"), "changed existing source held for reconciliation instead of overwriting booking");
    await importer.ImportAsync(changedReview.ReviewID);
    db.ChangeTracker.Clear();
    Check((await db.Reservations.FindAsync(synthetic.ReservationID))!.BookingStatusID != cancelID && await db.ReservationCharges.CountAsync(x => x.ReservationID == synthetic.ReservationID) == 4, "cancelled update to existing active booking never silently removes financial/manual data");
    var unknown = Fields("UNMAPPED"); unknown["部屋名称"] = "Unverified room"; unknown["部屋マスタ"] = "Unverified room";
    var unmappedFile = await Fixture("unmapped", unknown);
    var unmapped = await importer.PrepareAsync(unmappedFile, CsvFileEncoding.Auto, ',');
    Check(unmapped.HeldRows == 1, "unknown physical room held rather than created"); await importer.ImportAsync(unmapped.ReviewID);
    Check(!await db.Reservations.AnyAsync(x => x.ExternalReservationNumber == "UNMAPPED") && await db.Rooms.CountAsync() == 5, "audit-only held row never creates a reservation or physical room");
    var invalidFile = await Fixture("invalid-field-count", Fields("INVALID-FIELDS"));
    var invalidText = (await File.ReadAllTextAsync(invalidFile)).TrimEnd('\r', '\n') + ",extra-field\r\n";
    await File.WriteAllTextAsync(invalidFile, invalidText, new UTF8Encoding(true, true));
    var invalidReview = await importer.PrepareAsync(invalidFile, CsvFileEncoding.Auto, ',');
    Check(invalidReview.InvalidRows == 1, "extra-field source row is explicitly invalid, not silently mapped");
    await importer.ImportAsync(invalidReview.ReviewID);
    Check(!await db.Reservations.AnyAsync(x => x.ExternalReservationNumber == "INVALID-FIELDS") && await db.ImportRows.AnyAsync(x => x.ParseStatus == "Failed" && x.RawPayload!.Contains("extra-field")), "invalid source is durably audited with original extra values and no booking");
    var snapshotFile = await Fixture("snapshot", Fields("SNAPSHOT-ORIGINAL"));
    var snapshotBytes = await File.ReadAllBytesAsync(snapshotFile);
    var snapshotReview = await importer.PrepareAsync(snapshotFile, CsvFileEncoding.Auto, ',');
    await File.WriteAllTextAsync(snapshotFile, "changed after validation", new UTF8Encoding(false));
    await importer.ImportAsync(snapshotReview.ReviewID);
    var snapshotBooking = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "SNAPSHOT-ORIGINAL");
    var snapshotBatch = await db.ImportRows.Where(x => x.ReservationID == snapshotBooking.ReservationID).Select(x => x.ImportBatchID).SingleAsync();
    Check((await db.ImportBatches.FindAsync(snapshotBatch))!.SourceFileContent!.SequenceEqual(snapshotBytes), "file changed after validation cannot replace the reviewed byte snapshot");
    var dupFile = await Fixture("duplicate-keys", Fields("DUPKEY"), Fields("DUPKEY"));
    await Reject(() => importer.PrepareAsync(dupFile, CsvFileEncoding.Auto, ','), "duplicate OTA booking keys within a file reject the complete review");
    var unknownOta = Fields("UNKNOWN-OTA"); unknownOta["予約サイト名"] = "Unverified OTA";
    var unknownFile = await Fixture("unknown-ota", unknownOta);
    await Reject(() => importer.PrepareAsync(unknownFile, CsvFileEncoding.Auto, ','), "unverified OTA source rejects entire file without invented source identity");
    var stale = await importer.PrepareAsync(fixture, CsvFileEncoding.Auto, ','); importer.DiscardReview();
    await Reject(() => importer.ImportAsync(stale.ReviewID), "discarded review token cannot commit");
    var rollbackFile = await Fixture("rollback", Fields("ROLLBACK-1"), Fields("ROLLBACK-2"));
    var rollbackReview = await importer.PrepareAsync(rollbackFile, CsvFileEncoding.Auto, ',');
    var reservationsBeforeFailure = await db.Reservations.CountAsync(); var batchesBeforeFailure = await db.ImportBatches.CountAsync(); var customersBeforeFailure = await db.Customers.CountAsync();
    await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER dbo.Verifier_Abort ON dbo.Reservation AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE ExternalReservationNumber = N'ROLLBACK-2') THROW 51002, 'Synthetic verification abort', 1; END");
    await Reject(() => importer.ImportAsync(rollbackReview.ReviewID), "database failure on second booking surfaces an explicit import error");
    await db.Database.ExecuteSqlRawAsync("DROP TRIGGER dbo.Verifier_Abort");
    Check(await db.Reservations.CountAsync() == reservationsBeforeFailure && await db.ImportBatches.CountAsync() == batchesBeforeFailure && await db.Customers.CountAsync() == customersBeforeFailure, "failure rolls back first booking, customers, financials and audit batches as one transaction");
    var staleRoomFile = await Fixture("stale-room", Fields("STALE-ROOM"));
    var staleRoom = await importer.PrepareAsync(staleRoomFile, CsvFileEncoding.Auto, ',');
    await db.Rooms.Where(x => x.RoomName == "翔(Show)").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsActive, false));
    await Reject(() => importer.ImportAsync(staleRoom.ReviewID), "room disabled after validation aborts commit");
    await db.Rooms.Where(x => x.RoomName == "翔(Show)").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsActive, true));
    Check(!await db.Reservations.AnyAsync(x => x.ExternalReservationNumber == "STALE-ROOM") && await db.ImportBatches.CountAsync() == batchesBeforeFailure, "stale-room rejection rolls back its audit batch too");
    var raceFile = await Fixture("race", Fields("RACE"));
    var firstRace = await importer.PrepareAsync(raceFile, CsvFileEncoding.Auto, ',');
    var rival = new ReservationImportService(factory, reader, parser, provider.GetRequiredService<IRoomMappingService>(), NullLogger<ReservationImportService>.Instance);
    var secondRace = await rival.PrepareAsync(raceFile, CsvFileEncoding.Auto, ',');
    await importer.ImportAsync(firstRace.ReviewID);
    await Reject(() => rival.ImportAsync(secondRace.ReviewID), "another importer committing after review invalidates stale classification");
    Check(await db.Reservations.CountAsync(x => x.ExternalReservationNumber == "RACE") == 1, "concurrent prepared imports cannot duplicate booking identity");
    var cancelledToken = new CancellationToken(true);
    try { await importer.PrepareAsync(fixture, CsvFileEncoding.Auto, ',', cancellationToken: cancelledToken); throw new InvalidOperationException("FAIL: cancellation"); }
    catch (OperationCanceledException) { Check(true, "cancelled validation stops without any database writes"); }
    var vm = new CsvImportViewModel(provider.GetRequiredService<ICsvPreviewService>(), new FixedPicker(fixture), provider.GetRequiredService<IRoomSettingsService>(), NullLogger<CsvImportViewModel>.Instance, importer);
    await vm.SelectFileCommand.ExecuteAsync(null); Check(vm.TotalRows == 1 && !vm.CanImport, "ViewModel never enables import from preview alone");
    await vm.ValidateFileCommand.ExecuteAsync(null); Check(vm.ReviewRows.Count == 1 && !vm.CanImport, "prepared review still requires staff acknowledgment");
    vm.ReviewAcknowledged = true; Check(vm.CanImport, "reviewed duplicate can be explicitly audited without modifying staff data");
    vm.SelectedEncoding = vm.Encodings[2]; Check(!vm.CanImport && vm.ReviewRows.Count == 0 && !vm.HasPreview, "changing encoding invalidates import token and source preview");
    Console.WriteLine($"COMPLETED: {checks} checks. Real sample imported only into isolated verification database. Test database cleanup follows.");
}
finally
{
    if (databaseAttempted)
    {
        SqlConnection.ClearAllPools();
        await using var master = new SqlConnection(server + "Database=master;"); await master.OpenAsync();
        await using var drop = master.CreateCommand();
        drop.CommandText = $"IF DB_ID(N'{testName}') IS NOT NULL BEGIN ALTER DATABASE [{testName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{testName}]; END";
        await drop.ExecuteNonQueryAsync(); Console.WriteLine("CLEANUP: exact isolated verification database dropped.");
    }
    foreach (var file in files) File.Delete(file); Directory.Delete(directory);
}
sealed class FixedPicker(string path) : IFileDialogService { public string? SelectCsvFile() => path; }

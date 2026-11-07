using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gajyun.Core.Csv;
using Gajyun.Core.Dictionary;
using Gajyun.Core.Models;
using Gajyun.Core.Services;
using Gajyun.Data;
using Gajyun.Desktop.Services;
using Gajyun.Desktop.ViewModels;
using Gajyun.Desktop.Views;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

var checks = 0;
void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException("FAIL: " + name); Console.WriteLine("PASS: " + name); checks++; }
async Task Reject(Func<Task> action, string name) { try { await action(); } catch (ReservationImportException) { Check(true, name); return; } throw new InvalidOperationException("FAIL: " + name); }
const string server = "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;";
if (args.SequenceEqual(new[] { "--initialize-local" }))
{
    await using var local = new ServiceCollection().AddLogging().AddGajyunData(server + "Database=GuesthouseGajyunDb;").BuildServiceProvider();
    var initialized = await local.GetRequiredService<IDatabaseInitializationService>().InitializeAsync();
    if (!initialized.Succeeded) throw new InvalidOperationException("Local initialization failed: " + initialized);
    await using var databaseContext = await local.GetRequiredService<IDbContextFactory<GajyunDbContext>>().CreateDbContextAsync();
    Console.WriteLine($"LOCAL INITIALIZED: reservations={await databaseContext.Reservations.CountAsync()}, rooms={await databaseContext.Rooms.CountAsync()}, beds={await databaseContext.Beds.CountAsync()}, countries={await databaseContext.Countries.CountAsync()}, taxBrackets={await databaseContext.TaxBrackets.CountAsync()}, documentTypes={await databaseContext.DocumentTypes.CountAsync()}, OTAs={await databaseContext.OTAs.CountAsync()}");
    if (await databaseContext.Reservations.CountAsync() != 0 || await databaseContext.Countries.CountAsync() != 250 || await databaseContext.TaxBrackets.CountAsync() != 8)
        throw new InvalidOperationException("The reset database does not match the expected empty v1.3 foundation.");
    return;
}
var database = "Gajyun_RevisionVerification_" + Guid.NewGuid().ToString("N");
var root = Path.GetFullPath(".");
var directory = Path.Combine(AppContext.BaseDirectory, "Fixtures_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory); var fixtureFiles = new List<string>();
var attempt = false;
Dictionary<string, string> Fields(string number, string guest, string ota = "Booking.com", string booked = "2026-10-01 10:00:00", string arrival = "2026-10-20", string departure = "2026-10-21", string status = "予約", int price = 5700)
{
    var fields = CombinedExportReservationParser.RequiredHeaders.ToDictionary(x => x, _ => "");
    fields["予約サイト名"] = ota; fields["予約番号"] = number; fields["予約日時"] = booked; fields["予約区分"] = status;
    fields["チェックイン日"] = arrival; fields["チェックアウト日"] = departure;
    fields["宿泊者氏名"] = guest; fields["部屋名称"] = "翔(Show)"; fields["部屋マスタ"] = "翔(Show)";
    fields["部屋数"] = "1"; fields["大人人数"] = "2"; fields["子供人数"] = "0";
    var start = DateOnly.Parse(arrival); var end = DateOnly.Parse(departure); var count = end.DayNumber - start.DayNumber;
    fields["泊数"] = count.ToString(); fields["合計料金"] = (count * price).ToString();
    fields["詳細"] = string.Concat(Enumerable.Range(0, count).Select(i => $"[{start.AddDays(i):yyyy-MM-dd}] breakfast JPY {price}.00 2人")) + "deposit JPY 123.00mixed:1";
    return fields;
}
async Task<string> Fixture(string name, params Dictionary<string, string>[] records)
{
    var headers = records[0].Keys.ToArray(); string Quote(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
    var text = string.Join(',', headers.Select(Quote)) + "\r\n" + string.Join("\r\n", records.Select(r => string.Join(',', headers.Select(h => Quote(r.GetValueOrDefault(h, "")))))) + "\r\n";
    var path = Path.Combine(directory, name + ".csv"); await File.WriteAllTextAsync(path, text, new UTF8Encoding(true, true)); fixtureFiles.Add(path); return path;
}
try
{
    await using var provider = new ServiceCollection().AddLogging().AddGajyunData(server + "Database=" + database + ";").BuildServiceProvider();
    var dictionary = provider.GetRequiredService<IDataDictionaryService>().Load();
    Check(dictionary.SchemaVersion == "1.3" && dictionary.Tables.Count == 28 && dictionary.Tables.Any(x => x.Name == "StayMemo") && !dictionary.Tables.Any(x => x.Name == "ReservationMemo"), "offline dictionary matches all 27 revised business tables and financial view");
    Check(dictionary.Tables.Single(x => x.Name == "Reservation").Fields.Single(x => x.Name == "SourcePaymentMethodID").Version == "1.3" && !dictionary.Tables.Single(x => x.Name == "OTARoomMapping").Fields.Any(x => x.Name == "ExternalRoomCode"), "dictionary exposes source payment designation and omits removed room code");
    attempt = true;
    var init = await provider.GetRequiredService<IDatabaseInitializationService>().InitializeAsync(); Check(init.Succeeded, "rebuilt baseline creates an isolated SQL Server database");
    var factory = provider.GetRequiredService<IDbContextFactory<GajyunDbContext>>(); await using var db = await factory.CreateDbContextAsync();
    Check((await db.Database.GetAppliedMigrationsAsync()).Single().EndsWith("InitialSchemaV13"), "only rebuilt baseline migration is applied");
    var script = await File.ReadAllTextAsync(Path.Combine(root, "session-state", "files", "RevisionVerification", "InitialV13.sql"));
    await using (var scriptConnection = new SqlConnection(server + "Database=" + database + ";"))
    {
        await scriptConnection.OpenAsync();
        for (var run = 0; run < 2; run++)
            foreach (var batch in System.Text.RegularExpressions.Regex.Split(script, @"^GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                await using var command = scriptConnection.CreateCommand(); command.CommandText = batch; await command.ExecuteNonQueryAsync();
            }
    }
    Check((await db.Database.GetAppliedMigrationsAsync()).Count() == 1 && await db.Countries.CountAsync() == 250, "rebuilt idempotent migration script runs twice without duplicate seeds or view errors");
    Check(await db.Countries.CountAsync() == 250 && await db.Countries.AllAsync(x => x.FlagUrl != null) && (await db.Countries.SingleAsync(x => x.ISO2Code == "JP")).CountryName == "Japan", "all 250 flag-backed country/territory entries seeded by migration");
    Check(await db.OTAs.CountAsync() == 4 && await db.OTAs.AllAsync(x => x.LogoUrl != null) && (await db.OTAs.SingleAsync(x => x.OTAName == "Booking.com")).LogoUrl!.Contains("Booking-Logo.png"), "three real OTA logos and direct-sale logo have stable bundled URLs");
    Check(await db.DocumentTypes.AnyAsync(x => x.DocumentTypeName == "Driver's license") && await db.DocumentTypes.AnyAsync(x => x.DocumentTypeName == "Foreign residence card"), "driver license and foreign residence card document types seeded");
    var brackets = await db.TaxBrackets.OrderBy(x => x.TaxBracketID).ToArrayAsync();
    Check(brackets.Length == 8 && brackets.Select(x => x.TaxPerPersonNight).SequenceEqual(new decimal[] { 200, 500, 1000, 200, 400, 1000, 4000, 10000 }), "all eight approved Kyoto rates seeded exactly");
    Check(brackets.Where(x => x.TaxPolicyID == 1).Select(x => x.MinPriceInclusive).SequenceEqual(new decimal[] { 0, 20000, 50000 }) && brackets.Where(x => x.TaxPolicyID == 2).Select(x => x.MinPriceInclusive).SequenceEqual(new decimal[] { 0, 6000, 20000, 50000, 100000 }), "both policies use exact inclusive tier boundaries");
    Check(await db.Rooms.CountAsync() == 5 && await db.Rooms.SumAsync(x => (int)x.Capacity) == 18 && await db.Beds.CountAsync() == 14, "room capacity and dormitory bed seed remains exact");
    var importer = provider.GetRequiredService<IReservationImportService>();
    var samplePath = Path.Combine(root, "docs", "20261106154030.csv");
    var sample = await importer.PrepareAsync(samplePath, CsvFileEncoding.Auto, ',');
    Check(sample.ReadyRows == 257 && sample.CancelledRows == 74 && sample.HeldRows == 9 && sample.TotalRows == 340, "real sample retains exact 257 active, 74 cancelled, nine held outcomes");
    Check(sample.Rows.Where(x => x.BookedAt != null).Select(x => x.BookedAt).SequenceEqual(sample.Rows.Where(x => x.BookedAt != null).Select(x => x.BookedAt).Order()), "review is globally sorted by booking time, not OTA");
    Check(await db.Reservations.CountAsync() == 0 && await db.Customers.CountAsync() == 0, "review and comparison never persist guest data");
    Check(sample.Rows.Where(x => x.Disposition == ImportRowDisposition.Held).All(x => x.ComparisonFields.Count > 0 && !x.HasExistingReservation), "held new bookings show incoming source context without inventing an existing reservation");
    var result = await importer.ImportAsync(sample.ReviewID);
    Check(result.CreatedReservations == 331 && await db.Reservations.CountAsync() == 331, "approved real sample inserts exactly 331 eligible reservations without customer confirmation");
    var expectedCustomers = sample.Rows.Where(x => x.Disposition is ImportRowDisposition.Ready or ImportRowDisposition.Cancelled).Select(x => RoomNameNormalizer.Normalize(x.GuestName)).Distinct().Count();
    Check(await db.Customers.CountAsync() == expectedCustomers && expectedCustomers < 331, "real sample creates exactly one customer for each normalized eligible guest name");
    var bookedTimes = await db.Reservations.OrderBy(x => x.ReservationID).Select(x => x.BookedAt).ToArrayAsync();
    Check(bookedTimes.SequenceEqual(bookedTimes.Order()), "reservation identity insertion order follows BookedAt across all OTAs");
    Check(await db.StayReservations.CountAsync() == 257 && await db.Stays.CountAsync() <= 257, "all active bookings link to operational stays under the automatic-name rule");
    Check(await db.ReservationGroups.CountAsync() == 9 && await db.Reservations.AnyAsync(x => x.GroupID != null) && await db.ReservationGroups.AllAsync(x => x.LogoUrl!.Contains("VariousLogo.png")), "nine observed Booking.com prefix groups receive the bundled combined logo");
    Check(await db.OTARoomMappings.AnyAsync(x => x.OTAID != null) && await db.OTARoomMappings.CountAsync(x => x.OTAID == null) == 31, "approved source names learn OTA attribution without altering generic aliases");
    Check(await db.Reservations.AnyAsync(x => x.SourcePaymentMethodID == 5) && !await db.ReservationPayments.AnyAsync(), "Internet payment designation stored without inventing money received");
    Check(await db.ReservationTaxes.AllAsync(x => x.CalculationStatus == "PendingReview" && x.LegalTaxAmount == null && x.ChargedTaxAmount == null), "seeded rates do not assume breakfast-inclusive taxable base");
    var chargeCount = await db.ReservationCharges.CountAsync(); var groupCount = await db.ReservationGroups.CountAsync(); var stayCount = await db.Stays.CountAsync();
    var repeated = await importer.PrepareAsync(samplePath, CsvFileEncoding.Auto, ','); Check(repeated.DuplicateRows == 331, "unchanged import uses latest accepted source fingerprint");
    await importer.ImportAsync(repeated.ReviewID);
    Check(await db.Reservations.CountAsync() == 331 && await db.ReservationCharges.CountAsync() == chargeCount && await db.ReservationGroups.CountAsync() == groupCount && await db.Stays.CountAsync() == stayCount && await db.ImportRows.CountAsync() == 680, "reimport does not duplicate reservations, nightly lines, groups or stays");
    var adjacentFile = await Fixture("adjacent", Fields("SAME-1", "Synthetic Shared Guest", booked: "2026-10-01 08:00:00"),
        Fields("SAME-2", "Synthetic Shared Guest", ota: "Hostelworld", booked: "2026-10-01 09:00:00", arrival: "2026-10-21", departure: "2026-10-22"));
    var adjacentReview = await importer.PrepareAsync(adjacentFile, CsvFileEncoding.Auto, ',');
    var first = adjacentReview.Rows[0]; var second = adjacentReview.Rows[1];
    await importer.ImportAsync(adjacentReview.ReviewID);
    var pairIDs = await db.Reservations.Where(x => x.ExternalReservationNumber == "SAME-1" || x.ExternalReservationNumber == "SAME-2").Select(x => x.ReservationID).ToArrayAsync();
    var pairCustomers = await db.ReservationCustomers.Where(x => pairIDs.Contains(x.ReservationID)).Select(x => x.CustomerID).ToArrayAsync();
    var pairStays = await db.StayReservations.Where(x => pairIDs.Contains(x.ReservationID)).Select(x => x.StayID).ToArrayAsync();
    Check(pairCustomers.Distinct().Count() == 1 && pairStays.Distinct().Count() == 1 && (await db.Stays.FindAsync(pairStays[0]))!.EndDate == new DateOnly(2026, 10, 22), "same-name customer links continuous bookings across OTAs without merging reservations");
    Check(!await db.Reservations.AnyAsync(x => pairIDs.Contains(x.ReservationID) && x.GroupID != null), "same booking prefix on different OTAs never becomes one automatic group");
    db.StayMemos.Add(new StayMemo { StayID = pairStays[0], MemoText = "Synthetic stay note" }); await db.SaveChangesAsync();
    var distantFile = await Fixture("distant", Fields("DISTANT-1", "  synthetic　shared   guest  ", arrival: "2026-12-01", departure: "2026-12-02"));
    var distantReview = await importer.PrepareAsync(distantFile, CsvFileEncoding.Auto, ',');
    await importer.ImportAsync(distantReview.ReviewID);
    var distantCustomerID = await db.ReservationCustomers.Where(x => x.ReservationID == db.Reservations.Where(r => r.ExternalReservationNumber == "DISTANT-1").Select(r => r.ReservationID).Single()).Select(x => x.CustomerID).SingleAsync();
    Check(distantCustomerID == pairCustomers[0] && await db.Stays.CountAsync(x => x.CustomerID == pairCustomers[0]) == 2 && (await db.Customers.FindAsync(pairCustomers[0]))!.FullName == "Synthetic Shared Guest", "normalized names reuse existing customers across files without overwriting their original name; nonconsecutive stays remain separate");
    var bridgeEnds = await Fixture("bridge-ends", Fields("BRIDGE-1", "Synthetic Bridge Guest", arrival: "2027-01-01", departure: "2027-01-02"),
        Fields("BRIDGE-3", "Synthetic Bridge Guest", booked: "2026-10-01 11:00:00", arrival: "2027-01-03", departure: "2027-01-04"));
    var bridgeReview = await importer.PrepareAsync(bridgeEnds, CsvFileEncoding.Auto, ',');
    await importer.ImportAsync(bridgeReview.ReviewID);
    var bridgeCustomer = await db.Customers.SingleAsync(x => x.FullName == "Synthetic Bridge Guest");
    var bridgeStays = await db.Stays.Where(x => x.CustomerID == bridgeCustomer.CustomerID).ToArrayAsync();
    foreach (var stay in bridgeStays) db.StayMemos.Add(new StayMemo { StayID = stay.StayID, MemoText = "Bridge endpoint note" });
    await db.SaveChangesAsync();
    var bridgeMiddle = await Fixture("bridge-middle", Fields("BRIDGE-2", "Synthetic Bridge Guest", arrival: "2027-01-02", departure: "2027-01-03"));
    var middleReview = await importer.PrepareAsync(bridgeMiddle, CsvFileEncoding.Auto, ',');
    await importer.ImportAsync(middleReview.ReviewID);
    db.ChangeTracker.Clear();
    var mergedStay = await db.Stays.SingleAsync(x => x.CustomerID == bridgeCustomer.CustomerID);
    Check(mergedStay.StartDate == new DateOnly(2027, 1, 1) && mergedStay.EndDate == new DateOnly(2027, 1, 4) && await db.StayReservations.CountAsync(x => x.StayID == mergedStay.StayID) == 3 && await db.StayMemos.CountAsync(x => x.StayID == mergedStay.StayID) == 2, "confirmed bridge booking joins both stays while preserving all stay notes and original reservations");
    var updateInitial = Fields("UPDATE-1", "Synthetic Update Guest");
    var initialFile = await Fixture("initial-update", updateInitial); var initialReview = await importer.PrepareAsync(initialFile, CsvFileEncoding.Auto, ','); await importer.ImportAsync(initialReview.ReviewID);
    var updateBooking = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "UPDATE-1");
    db.ReservationPayments.Add(new ReservationPayment { ReservationID = updateBooking.ReservationID, Direction = "Inflow", CounterpartyType = "Guest", PaymentKind = "Payment", Amount = 300m });
    db.ReservationCharges.Add(new ReservationCharge { ReservationID = updateBooking.ReservationID, ChargeType = "Towel", UnitPrice = 100m });
    var stayID = await db.StayReservations.Where(x => x.ReservationID == updateBooking.ReservationID).Select(x => x.StayID).SingleAsync();
    db.StayMemos.Add(new StayMemo { StayID = stayID, MemoText = "Staff note retained" }); await db.SaveChangesAsync();
    var updateFile = await Fixture("update", Fields("UPDATE-1", "Synthetic Update Guest", departure: "2026-10-22", price: 6000));
    var updateReview = await importer.PrepareAsync(updateFile, CsvFileEncoding.Auto, ',');
    Check(updateReview.ChangedRows == 1 && updateReview.Rows[0].Message.Contains("Departure") && updateReview.Rows[0].Message.Contains("5700") && updateReview.Rows[0].Message.Contains("12000"), "changed booking exposes old/new dates and room-total prices before approval");
    var departureComparison = updateReview.Rows[0].ComparisonFields.Single(x => x.Label == "Departure");
    var amountComparison = updateReview.Rows[0].ComparisonFields.Single(x => x.Label == "Accommodation JPY");
    Check(updateReview.Rows[0].HasExistingReservation && departureComparison.ExistingValue == "2026-10-21" && departureComparison.IncomingValue == "2026-10-22" && departureComparison.IsDifferent && amountComparison.ExistingValue == "5,700.00" && amountComparison.IncomingValue == "12,000.00", "comparison shows exact existing and incoming dates and room totals, not a summary-only proxy");
    Check(!updateReview.Rows[0].ComparisonFields.Single(x => x.Label == "Booking").IsDifferent && updateReview.Rows[0].ComparisonFields.Single(x => x.Label == "Night 2026-10-21 JPY").IncomingValue == "6,000.00", "comparison distinguishes unchanged identifiers and exposes added nightly prices");
    await importer.ImportAsync(updateReview.ReviewID); db.ChangeTracker.Clear();
    Check((await db.Reservations.FindAsync(updateBooking.ReservationID))!.CheckOutDate == new DateOnly(2026, 10, 22) && await db.ReservationCharges.Where(x => x.ReservationID == updateBooking.ReservationID && x.ChargeType == "Accommodation").SumAsync(x => x.Amount) == 12000m, "approved changed booking updates source-owned dates and nightly charges");
    Check(await db.ReservationPayments.AnyAsync(x => x.ReservationID == updateBooking.ReservationID && x.Amount == 300m) && await db.ReservationCharges.AnyAsync(x => x.ReservationID == updateBooking.ReservationID && x.ChargeType == "Towel") && await db.StayMemos.AnyAsync(x => x.StayID == stayID && x.MemoText == "Staff note retained"), "approved update preserves actual payments, manual extras and stay notes");
    var cancelledFile = await Fixture("cancel-active", Fields("UPDATE-1", "Synthetic Update Guest", departure: "2026-10-22", price: 6000, status: "キャンセル"));
    var cancellation = await importer.PrepareAsync(cancelledFile, CsvFileEncoding.Auto, ',');
    Check(cancellation.CancellationUpdates == 1 && cancellation.Rows[0].Message.Contains("Previously active"), "cancelled source explicitly shows previously-active existing booking");
    await importer.ImportAsync(cancellation.ReviewID); db.ChangeTracker.Clear();
    var cancelledStatus = await db.BookingStatuses.Where(x => x.StatusName == "Cancelled").Select(x => x.BookingStatusID).SingleAsync();
    Check((await db.Reservations.FindAsync(updateBooking.ReservationID))!.BookingStatusID == cancelledStatus && await db.ReservationPayments.AnyAsync(x => x.ReservationID == updateBooking.ReservationID) && await db.StayMemos.AnyAsync(x => x.StayID == stayID), "approved cancellation changes lifecycle status without deleting financial/stay history");
    var historical = await importer.PrepareAsync(initialFile, CsvFileEncoding.Auto, ',');
    Check(historical.DuplicateRows == 0, "an old historical fingerprint is not incorrectly classified as current unchanged data");
    var manualBooking = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "DISTANT-1"); manualBooking.ExpectedGuestCount = 3; await db.SaveChangesAsync();
    var manualFile = await Fixture("manual-conflict", Fields("DISTANT-1", "Synthetic Shared Guest", arrival: "2026-12-01", departure: "2026-12-02", price: 6200));
    var manualReview = await importer.PrepareAsync(manualFile, CsvFileEncoding.Auto, ','); Check(manualReview.HeldRows == 1 && manualReview.Rows[0].Message.Contains("Staff changed"), "manual field edits block unsafe automatic overwrite");
    var guestsComparison = manualReview.Rows[0].ComparisonFields.Single(x => x.Label == "Guests");
    Check(manualReview.Rows[0].HasExistingReservation && guestsComparison.ExistingValue == "3" && guestsComparison.IncomingValue == "2" && guestsComparison.IsDifferent, "held comparison uses current staff-edited database values, not historical source values");
    var conflictFields = Fields("CONFLICT-1", "Synthetic Room Conflict"); conflictFields["部屋名称"] = "翔(Show)"; conflictFields["部屋マスタ"] = "順(Jyun)";
    var conflictFile = await Fixture("room-conflict", conflictFields); var conflict = await importer.PrepareAsync(conflictFile, CsvFileEncoding.Auto, ',');
    Check(conflict.HeldRows == 1 && conflict.Rows[0].RoomChoices.Count == 5, "conflicting external/master room names ask which physical room is correct");
    var show = await db.Rooms.SingleAsync(x => x.RoomName == "翔(Show)");
    await importer.ImportAsync(conflict.ReviewID, [new(conflict.Rows[0].RowNumber, RoomID: show.RoomID)]);
    Check(await db.Reservations.AnyAsync(x => x.ExternalReservationNumber == "CONFLICT-1") && await db.Rooms.CountAsync() == 5, "explicit room resolution imports without creating a physical room");
    var raceFile = await Fixture("race-update", Fields("CONFLICT-1", "Synthetic Room Conflict", price: 6000));
    var raceReview = await importer.PrepareAsync(raceFile, CsvFileEncoding.Auto, ',');
    var conflictBooking = await db.Reservations.SingleAsync(x => x.ExternalReservationNumber == "CONFLICT-1"); conflictBooking.ExpectedGuestCount = 3; await db.SaveChangesAsync();
    if (raceReview.ChangedRows > 0) await Reject(() => importer.ImportAsync(raceReview.ReviewID), "staff edit after review invalidates an approved update transaction");
    else Check(raceReview.HeldRows == 1, "previously conflicting room source requires reconciliation before automatic changes");
    var roomDecisionChanged = false;
    var unresolvedRow = new ImportReviewRowViewModel(new ImportReviewRow(1, "Booking.com", ImportRowDisposition.Changed, "Choose a room", 1, 5700m)
        { RoomChoices = [new(show.RoomID, show.RoomName)] }, () => roomDecisionChanged = true);
    Check(unresolvedRow.IsUnresolved, "changed booking with a room conflict still requires a room choice");
    unresolvedRow.SelectedRoom = unresolvedRow.RoomChoices[0];
    Check(!unresolvedRow.IsUnresolved && unresolvedRow.Decision.RoomID == show.RoomID && roomDecisionChanged, "room resolution clears its question and invalidates batch approval");
    var sourceParser = provider.GetRequiredService<ICsvReservationParser>();
    var noPrice = Fields("NOPRICE-1", "Synthetic No Price", status: "キャンセル"); noPrice["詳細"] = @"notes with a backslash \ but no price";
    Check(sourceParser.Parse(noPrice).Reservation?.SourcePaymentMethod is null, "a backslash without a price never invents an Internet-payment designation");
    var staleInitialFile = await Fixture("stale-update-initial", Fields("STALEUPDATE-1", "Synthetic Stale Update"));
    var staleInitial = await importer.PrepareAsync(staleInitialFile, CsvFileEncoding.Auto, ','); await importer.ImportAsync(staleInitial.ReviewID);
    var staleChangedFile = await Fixture("stale-update-changed", Fields("STALEUPDATE-1", "Synthetic Stale Update", price: 6500));
    var staleChanged = await importer.PrepareAsync(staleChangedFile, CsvFileEncoding.Auto, ',');
    Check(staleChanged.ChangedRows == 1, "clean source update is eligible before a concurrent staff edit");
    await db.Reservations.Where(x => x.ExternalReservationNumber == "STALEUPDATE-1").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ExpectedGuestCount, (short?)3));
    await Reject(() => importer.ImportAsync(staleChanged.ReviewID), "staff edit after review aborts the stale approved update");
    Check(await db.ReservationCharges.Where(x => x.ReservationID == db.Reservations.Where(r => r.ExternalReservationNumber == "STALEUPDATE-1").Select(r => r.ReservationID).Single()).SumAsync(x => x.Amount) == 5700m, "stale update preserves original finances");
    var cashMethodID = await db.PaymentMethods.Where(x => x.MethodName == "Cash").Select(x => x.PaymentMethodID).SingleAsync();
    await db.Reservations.Where(x => x.ExternalReservationNumber == "STALEUPDATE-1").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ExpectedGuestCount, (short?)2).SetProperty(x => x.SourcePaymentMethodID, (int?)cashMethodID));
    var manualMethodReview = await importer.PrepareAsync(staleChangedFile, CsvFileEncoding.Auto, ',');
    Check(manualMethodReview.HeldRows == 1 && manualMethodReview.Rows[0].Message.Contains("Staff changed the source payment method"), "staff-edited payment designation cannot be overwritten by source updates");
    var auditFile = await Fixture("audit-initial", Fields("AUDIT-1", "Synthetic Audit Guest"));
    var auditReview = await importer.PrepareAsync(auditFile, CsvFileEncoding.Auto, ','); await importer.ImportAsync(auditReview.ReviewID);
    var auditReservationID = await db.Reservations.Where(x => x.ExternalReservationNumber == "AUDIT-1").Select(x => x.ReservationID).SingleAsync();
    await db.ImportRows.Where(x => x.ReservationID == auditReservationID && x.ParseStatus == "Processed").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RawPayload, "{"));
    var damagedAuditFile = await Fixture("audit-changed", Fields("AUDIT-1", "Synthetic Audit Guest", price: 6200));
    await Reject(() => importer.PrepareAsync(damagedAuditFile, CsvFileEncoding.Auto, ','), "damaged source audit surfaces a visible import error instead of overwriting a booking");
    await db.ImportRows.Where(x => x.ReservationID == auditReservationID && x.ParseStatus == "Processed").ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RawPayload, "{}"));
    var missingAuditReview = await importer.PrepareAsync(damagedAuditFile, CsvFileEncoding.Auto, ',');
    Check(missingAuditReview.HeldRows == 1 && missingAuditReview.Rows[0].Message.Contains("Previous source is unavailable"), "missing historical source fields explicitly hold the changed booking for reconciliation");
    var staleFile = await Fixture("stale", Fields("STALE-1", "Synthetic Stale Guest")); var staleReview = await importer.PrepareAsync(staleFile, CsvFileEncoding.Auto, ','); importer.DiscardReview();
    await Reject(() => importer.ImportAsync(staleReview.ReviewID), "discarded review cannot commit");
    var failureFile = await Fixture("failure", Fields("FAIL-1", "Synthetic First"), Fields("FAIL-2", "Synthetic Second")); var failureReview = await importer.PrepareAsync(failureFile, CsvFileEncoding.Auto, ',');
    var bookingsBefore = await db.Reservations.CountAsync(); var batchesBefore = await db.ImportBatches.CountAsync();
    await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER dbo.VerifierAbort ON dbo.Reservation AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE ExternalReservationNumber=N'FAIL-2') THROW 51002, 'Synthetic abort', 1; END");
    await Reject(() => importer.ImportAsync(failureReview.ReviewID), "SQL failure surfaces a visible import error"); await db.Database.ExecuteSqlRawAsync("DROP TRIGGER dbo.VerifierAbort");
    Check(await db.Reservations.CountAsync() == bookingsBefore && await db.ImportBatches.CountAsync() == batchesBefore, "failure rolls back earlier bookings, stay/group writes and audit batches together");
    var japaneseFile = await Fixture("japanese-names", Fields("JP-1", "山田　太郎"), Fields("JP-2", "山田 太郎", ota: "Hostelworld"), Fields("JP-3", "山田 次郎"));
    var japaneseReview = await importer.PrepareAsync(japaneseFile, CsvFileEncoding.Auto, ','); await importer.ImportAsync(japaneseReview.ReviewID);
    var japaneseLinks = await (from booking in db.Reservations join link in db.ReservationCustomers on booking.ReservationID equals link.ReservationID
        where booking.ExternalReservationNumber == "JP-1" || booking.ExternalReservationNumber == "JP-2" || booking.ExternalReservationNumber == "JP-3"
        select new { booking.ExternalReservationNumber, link.CustomerID }).ToArrayAsync();
    Check(japaneseLinks.Single(x => x.ExternalReservationNumber == "JP-1").CustomerID == japaneseLinks.Single(x => x.ExternalReservationNumber == "JP-2").CustomerID && japaneseLinks.Single(x => x.ExternalReservationNumber == "JP-1").CustomerID != japaneseLinks.Single(x => x.ExternalReservationNumber == "JP-3").CustomerID, "Japanese whitespace-normalized names share a customer across OTAs, but different full names stay separate");
    var olderCustomer = new Customer { FullName = "Synthetic Duplicate Guest", Email = "staff-preserved@example.invalid" };
    db.Customers.Add(olderCustomer); await db.SaveChangesAsync();
    var newerCustomer = new Customer { FullName = "synthetic duplicate guest", Nickname = "Keep this separate old record" };
    db.Customers.Add(newerCustomer); await db.SaveChangesAsync();
    var duplicateNameFile = await Fixture("duplicate-existing-names", Fields("OLD-DUP-1", "SYNTHETIC DUPLICATE GUEST"));
    var duplicateNameReview = await importer.PrepareAsync(duplicateNameFile, CsvFileEncoding.Auto, ','); await importer.ImportAsync(duplicateNameReview.ReviewID);
    var chosenCustomerID = await db.ReservationCustomers.Where(x => x.ReservationID == db.Reservations.Where(r => r.ExternalReservationNumber == "OLD-DUP-1").Select(r => r.ReservationID).Single()).Select(x => x.CustomerID).SingleAsync();
    db.ChangeTracker.Clear();
    Check(chosenCustomerID == olderCustomer.CustomerID && (await db.Customers.FindAsync(olderCustomer.CustomerID))!.Email == "staff-preserved@example.invalid" && (await db.Customers.FindAsync(newerCustomer.CustomerID))!.Nickname == "Keep this separate old record", "historical duplicate names reuse the oldest identity without merging or overwriting either staff-owned record");
    var cancelledFollowup = await Fixture("cancelled-followup", Fields("UPDATE-2", "Synthetic Update Guest", arrival: "2026-10-22", departure: "2026-10-23"));
    var followupReview = await importer.PrepareAsync(cancelledFollowup, CsvFileEncoding.Auto, ','); await importer.ImportAsync(followupReview.ReviewID);
    var followupStayID = await db.StayReservations.Where(x => x.ReservationID == db.Reservations.Where(r => r.ExternalReservationNumber == "UPDATE-2").Select(r => r.ReservationID).Single()).Select(x => x.StayID).SingleAsync();
    Check(followupStayID != stayID, "automatic name reuse never links a new visit to the cancelled booking's retained stay");
    var agodaCustomer = new Customer { FullName = "Synthetic Held Agoda Guest" }; db.Customers.Add(agodaCustomer); await db.SaveChangesAsync();
    var agodaID = await db.OTAs.Where(x => x.OTAName == "Agoda").Select(x => x.OTAID).SingleAsync();
    var confirmedID = await db.BookingStatuses.Where(x => x.StatusName == "Confirmed").Select(x => x.BookingStatusID).SingleAsync();
    var agodaBooking = new Reservation { OTAID = agodaID, ExternalReservationNumber = "HELD-AGODA-1", BookingStatusID = confirmedID, CheckInDate = new(2026, 10, 19), CheckOutDate = new(2026, 10, 20), ExpectedGuestCount = 1 };
    db.Reservations.Add(agodaBooking); await db.SaveChangesAsync();
    db.ReservationCustomers.Add(new ReservationCustomer { ReservationID = agodaBooking.ReservationID, CustomerID = agodaCustomer.CustomerID, IsLeadGuest = true });
    db.ReservationCharges.Add(new ReservationCharge { ReservationID = agodaBooking.ReservationID, ChargeType = "Accommodation", ServiceDate = new(2026, 10, 19), UnitPrice = 4000m }); await db.SaveChangesAsync();
    var agodaFile = await Fixture("held-agoda-existing", Fields("HELD-AGODA-1", "Synthetic Held Agoda Guest", ota: "Agoda"));
    var agodaReview = await importer.PrepareAsync(agodaFile, CsvFileEncoding.Auto, ',');
    var agodaArrival = agodaReview.Rows[0].ComparisonFields.Single(x => x.Label == "Arrival");
    var agodaAmount = agodaReview.Rows[0].ComparisonFields.Single(x => x.Label == "Accommodation JPY");
    Check(agodaReview.HeldRows == 1 && agodaReview.Rows[0].HasExistingReservation && agodaArrival.ExistingValue == "2026-10-19" && agodaArrival.IncomingValue == "2026-10-20" && agodaAmount.ExistingValue == "4,000.00" && agodaAmount.IncomingValue == "Source total (unverified): 5700", "unverified OTA rows compare actual existing bookings against clearly labeled raw incoming values");
    var settings = provider.GetRequiredService<IRoomSettingsService>();
    var rowVM = new RoomPolicyRowViewModel((await settings.LoadAsync()).Rooms.Single(x => x.RoomID == show.RoomID), [new(null, "None"), new(1, "2025"), new(2, "2026")], settings, (_, _) => { });
    rowVM.SelectedPolicy = rowVM.Policies[2]; await rowVM.SaveCommand.ExecuteAsync(null);
    Check((await settings.LoadAsync()).Rooms.Single(x => x.RoomID == show.RoomID).DefaultTaxPolicyID == 2 && rowVM.Status == "Saved", "per-room policy dropdown saves via service and gives direct feedback");
    var vm = new CsvImportViewModel(provider.GetRequiredService<ICsvPreviewService>(), new FixedPicker(samplePath), settings, NullLogger<CsvImportViewModel>.Instance, importer);
    await vm.SelectFileCommand.ExecuteAsync(null);
    Check(vm.ReviewRows.Count == 340 && !vm.CanImport && vm.NewBookings.Count() == 0 && vm.Questions.Count() >= 9, "choosing CSV automatically prepares a concise review while keeping import unapproved");
    vm.ReviewAcknowledged = true; Check(vm.CanImport, "batch approval enables unchanged-booking audit after required questions resolved");
    vm.SelectedEncoding = vm.Encodings[2]; Check(!vm.CanImport && vm.ReviewRows.Count == 0, "changing encoding clears approval and prepared decisions");
    Exception? uiError = null;
    var ui = new Thread(() =>
    {
        try
        {
            static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
            {
                for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
                {
                    var child = VisualTreeHelper.GetChild(parent, index);
                    yield return child;
                    foreach (var nested in Descendants(child)) yield return nested;
                }
            }
            var app = new Application();
            vm.ReviewRows.Add(new(manualReview.Rows[0], () => { }));
            vm.ReviewRows.Add(new(conflict.Rows[0], () => { }));
            var csvView = new CsvImportView { DataContext = vm };
            var settingsVM = new SettingsViewModel(settings, NullLogger<SettingsViewModel>.Instance);
            settingsVM.LoadCommand.ExecuteAsync(null).GetAwaiter().GetResult();
            var settingsView = new SettingsView { DataContext = settingsVM };
            var window = new Window { Width = 1200, Height = 800, Content = csvView, Title = "Gajyun isolated revision verification" };
            window.Show(); window.UpdateLayout();
            var tabs = Descendants(csvView).OfType<TabControl>().Single();
            tabs.Items.OfType<TabItem>().Single(x => Equals(x.Header, "Questions / held")).IsSelected = true; window.UpdateLayout();
            var bookingList = (ListBox)csvView.FindName("QuestionBookings");
            var comparisonGrid = Descendants(csvView).OfType<DataGrid>().Single(x => ReferenceEquals(x.ItemsSource, manualReview.Rows[0].ComparisonFields));
            var questionText = Descendants(csvView).OfType<TextBlock>().Single(x => x.Text == "Review / question");
            Check(ReferenceEquals(bookingList.SelectedItem, vm.ReviewRows[0]) && comparisonGrid.Columns[1].Header.Equals("Existing reservation") && comparisonGrid.Columns[2].Header.Equals("Incoming CSV reservation") && comparisonGrid.TransformToAncestor(csvView).Transform(new Point()).Y < questionText.TransformToAncestor(csvView).Transform(new Point()).Y, "rendered held screen shows both reservations before the question");
            var guestRow = (DataGridRow)comparisonGrid.ItemContainerGenerator.ContainerFromItem(guestsComparison);
            Check(((SolidColorBrush)guestRow.Background).Color == Color.FromRgb(255, 244, 214), "rendered comparison highlights the actual changed database guest count");
            bookingList.SelectedIndex = 1; window.UpdateLayout();
            Check(Descendants(csvView).OfType<DataGrid>().Any(x => ReferenceEquals(x.ItemsSource, conflict.Rows[0].ComparisonFields)) && Descendants(csvView).OfType<ComboBox>().Any(x => ReferenceEquals(x.ItemsSource, conflict.Rows[0].RoomChoices)), "selecting another held booking updates both its comparison and room question");
            Check(!Descendants(csvView).OfType<TextBlock>().Any(x => x.Text.Contains("same customer?", StringComparison.OrdinalIgnoreCase)), "rendered questions contain no obsolete customer-identity prompt");
            window.Content = settingsView; window.UpdateLayout();
            window.Close();
            foreach (var country in db.Countries.AsNoTracking().ToArray())
                if (Application.GetResourceStream(new Uri(country.FlagUrl!, UriKind.Absolute)) is not { Stream: not null }) throw new InvalidOperationException("Missing bundled flag asset.");
            foreach (var ota in db.OTAs.AsNoTracking().ToArray())
                if (Application.GetResourceStream(new Uri(ota.LogoUrl!, UriKind.Absolute)) is not { Stream: not null }) throw new InvalidOperationException("Missing bundled OTA asset.");
            if (Application.GetResourceStream(new Uri("pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/VariousLogo.png")) is not { Stream: not null }) throw new InvalidOperationException("Missing combined asset.");
            app.Shutdown();
        }
        catch (Exception exception) { uiError = exception; }
    });
    ui.SetApartmentState(ApartmentState.STA); ui.Start(); ui.Join();
    Check(uiError is null, "actual WPF views render and all persisted flag/logo URLs resolve: " + uiError?.Message);
    db.ChangeTracker.Clear();
    await Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(db).MigrateAsync("0");
    Check(!(await db.Database.GetAppliedMigrationsAsync()).Any(), "rebuilt migration rollback removes the view and dependent tables successfully");
    Check((await provider.GetRequiredService<IDatabaseInitializationService>().InitializeAsync()).Succeeded && await db.Countries.CountAsync() == 250 && await db.Rooms.CountAsync() == 5 && await db.Reservations.CountAsync() == 0, "database reinitializes cleanly through the rebuilt baseline after rollback");
    Console.WriteLine($"COMPLETED {checks} revised-schema/import/UI checks; test database cleanup follows.");
}
finally
{
    if (attempt)
    {
        SqlConnection.ClearAllPools(); await using var master = new SqlConnection(server + "Database=master;"); await master.OpenAsync();
        await using var command = master.CreateCommand(); command.CommandText = $"IF DB_ID(N'{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END";
        await command.ExecuteNonQueryAsync(); Console.WriteLine("CLEANUP: exact isolated revision database removed.");
    }
    foreach (var file in fixtureFiles) File.Delete(file); Directory.Delete(directory);
}
sealed class FixedPicker(string path) : IFileDialogService { public string? SelectCsvFile() => path; }

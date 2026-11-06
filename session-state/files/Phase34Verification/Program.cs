using System.IO;
using System.Text;
using Gajyun.Core.Csv;
using Gajyun.Core.Dictionary;
using Gajyun.Core.Models;
using Gajyun.Core.Services;
using Gajyun.Data;
using Gajyun.Data.Seeding;
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
async Task Reject(Func<Task> operation, string message)
{
    try { await operation(); }
    catch (CsvPreviewException) { Check(true, message); return; }
    throw new InvalidOperationException("FAIL: " + message);
}

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var directory = Path.Combine(Path.GetTempPath(), "GajyunPhase34_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
var files = new List<string>();
async Task<string> Fixture(string name, string text, Encoding encoding)
{
    var path = Path.Combine(directory, name);
    await File.WriteAllTextAsync(path, text, encoding);
    files.Add(path);
    return path;
}
var service = new CsvPreviewService();
try
{
    const string source = "予約番号,部屋,詳細,任意\r\nR1,順(Jyun),\"hello, \"\"quoted\"\"\r\nsecond line\",\r\nR2,雅(Miyabi),日本語,optional\r\n";
    var utf = await Fixture("utf8.csv", source, new UTF8Encoding(false, true));
    var result = await service.ReadAsync(utf, CsvFileEncoding.Auto, ',');
    Check(result.Headers.SequenceEqual(new[] { "予約番号", "部屋", "詳細", "任意" }), "Japanese headers preserved");
    Check(result.TotalRows == 2 && result.ValidRows == 2 && result.InvalidRows == 0, "quoted multiline input counts records, not physical lines");
    Check(result.Records[0].Values[2] == "hello, \"quoted\"\r\nsecond line", "quoted commas, escaped quotes and embedded newlines decoded");
    Check(result.Records[0].Values[3] == "", "empty optional value preserved");
    Check(result.Records[0].RawRecord.Contains("\"hello, \"\"quoted\"\"\r\nsecond line\""), "raw quoting and unknown text preserved");
    Check(result.EncodingDescription.StartsWith("UTF-8"), "strict UTF-8 detection");
    var bom = await Fixture("bom.csv", source, new UTF8Encoding(true, true));
    result = await service.ReadAsync(bom, CsvFileEncoding.Auto, ',');
    Check(result.Headers[0] == "予約番号" && result.EncodingDescription.Contains("BOM"), "UTF-8 BOM excluded from first header");
    await Reject(() => service.ReadAsync(bom, CsvFileEncoding.Windows31J, ','), "explicit CP932 rejects UTF-8 BOM");
    var sjis = await Fixture("cp932.csv", source, Encoding.GetEncoding(932));
    result = await service.ReadAsync(sjis, CsvFileEncoding.Auto, ',');
    Check(result.Headers[0] == "予約番号" && result.Records[1].Values[2] == "日本語" && result.EncodingDescription.StartsWith("Windows-31J"), "CP932 auto-detection reads Japanese text exactly");
    result = await service.ReadAsync(sjis, CsvFileEncoding.Windows31J, ',');
    Check(result.Records[0].Values[1] == "順(Jyun)", "explicit Windows-31J supported");
    await Reject(() => service.ReadAsync(sjis, CsvFileEncoding.Utf8, ','), "explicit UTF-8 rejects invalid CP932 bytes");
    var mismatch = await Fixture("mismatch.csv", "a,b\n1\n2,3,4\n5,6\n", Encoding.UTF8);
    result = await service.ReadAsync(mismatch, CsvFileEncoding.Auto, ',');
    Check(result.TotalRows == 3 && result.InvalidRows == 2 && result.ValidRows == 1, "short and long CSV records flagged, not silently padded");
    Check(result.Records[1].Values.SequenceEqual(new[] { "2", "3", "4" }), "extra unknown fields retained");
    var many = await Fixture("many.csv", "a,b\n" + string.Concat(Enumerable.Range(0, 501).Select(i => $"{i},x\n")), Encoding.UTF8);
    result = await service.ReadAsync(many, CsvFileEncoding.Auto, ',');
    Check(result.TotalRows == 501 && result.ValidRows == 501 && result.Records.Count == 200 && result.IsTruncated, "all rows counted while preview capped at exactly 200");
    var header = await Fixture("header.csv", "a,b\n", Encoding.UTF8);
    result = await service.ReadAsync(header, CsvFileEncoding.Auto, ',');
    Check(result.TotalRows == 0 && result.Headers.Length == 2, "header-only CSV supported");
    var malformed = await Fixture("malformed.csv", "a,b\n1,\"unclosed\n", Encoding.UTF8);
    await Reject(() => service.ReadAsync(malformed, CsvFileEncoding.Auto, ','), "unterminated quoted field rejected");
    var stray = await Fixture("stray.csv", "a,b\n1,x\"y\n", Encoding.UTF8);
    await Reject(() => service.ReadAsync(stray, CsvFileEncoding.Auto, ','), "stray quote rejected");
    var duplicate = await Fixture("duplicate.csv", "a,a\n1,2\n", Encoding.UTF8);
    await Reject(() => service.ReadAsync(duplicate, CsvFileEncoding.Auto, ','), "duplicate headers rejected");
    var blank = await Fixture("blank.csv", "a,\n1,2\n", Encoding.UTF8);
    await Reject(() => service.ReadAsync(blank, CsvFileEncoding.Auto, ','), "empty headers rejected");
    var empty = await Fixture("empty.csv", "", Encoding.UTF8);
    await Reject(() => service.ReadAsync(empty, CsvFileEncoding.Auto, ','), "empty CSV rejected");
    var semicolon = await Fixture("semi.csv", "a;b\n1;\"x;y\"\n", Encoding.UTF8);
    result = await service.ReadAsync(semicolon, CsvFileEncoding.Auto, ';');
    Check(result.Records[0].Values[1] == "x;y", "explicit semicolon delimiter supported");
    var tab = await Fixture("tab.csv", "a\tb\n1\t日本語\n", Encoding.UTF8);
    result = await service.ReadAsync(tab, CsvFileEncoding.Auto, '\t');
    Check(result.Records[0].Values[1] == "日本語", "explicit tab delimiter supported");
    using (var cancellation = new CancellationTokenSource())
    {
        cancellation.Cancel();
        try { await service.ReadAsync(many, CsvFileEncoding.Auto, ',', cancellationToken: cancellation.Token); throw new Exception("Cancellation ignored"); }
        catch (OperationCanceledException) { Check(true, "CSV reads honor cancellation"); }
    }
    Check(RoomNameNormalizer.Normalize("　女性　 ドミトリー４bed  ") == "女性 ドミトリー4BED", "Japanese whitespace and full-width Unicode normalize consistently");
    var mapper = new RoomMappingService();
    OTARoomMapping Alias(int room, int? ota, string name, string? code = null) => new() { RoomID = room, OTAID = ota, ExternalRoomName = name, NormalizedName = RoomNameNormalizer.Normalize(name), ExternalRoomCode = code };
    var aliases = new[] { Alias(1, null, "順 ４bed"), Alias(2, 9, "順 ４bed", "EXACT"), Alias(3, null, "Shared"), Alias(4, null, "Shared") };
    Check(mapper.Match(9, "EXACT", "unknown", aliases).RoomID == 2, "OTA exact code has highest priority");
    Check(mapper.Match(9, null, "順　4bed", aliases).RoomID == 2, "normalized OTA name precedes generic alias");
    Check(mapper.Match(8, null, "順　4bed", aliases).RoomID == 1, "unique generic Japanese alias matched");
    Check(mapper.Match(8, null, "Shared", aliases).Status == RoomMatchStatus.Ambiguous, "ambiguous generic alias never chooses a room");
    Check(mapper.Match(null, null, "Unknown", aliases).Status == RoomMatchStatus.Unmapped, "unknown alias never creates a room");

    var offlineServices = new ServiceCollection().AddLogging().AddGajyunData("Server=nonexistent.invalid;Database=OfflineOnly;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1;").BuildServiceProvider();
    using (offlineServices)
    {
        var dictionary = offlineServices.GetRequiredService<IDataDictionaryService>().Load();
        Check(dictionary.Tables.Count(x => !x.IsView) == 27 && dictionary.Tables.Count(x => x.IsView) == 1, "dictionary loads all 27 tables plus view without SQL connectivity");
        Check(dictionary.Tables.SelectMany(x => x.Fields).All(x => !string.IsNullOrWhiteSpace(x.Description)), "every implemented field has business documentation");
        var roomDictionary = dictionary.Tables.Single(x => x.Name == "Room");
        Check(roomDictionary.Fields.Single(x => x.Name == "Capacity").SqlType == "smallint" && roomDictionary.Fields.Single(x => x.Name == "Capacity").Version == "1.1", "dictionary field types and versions match EF");
        Check(dictionary.Tables.Single(x => x.Name == "ReservationCharge").Fields.Single(x => x.Name == "Amount").DefaultValue.Contains("Computed"), "dictionary explains persisted computed amount");
        Check(dictionary.Tables.Single(x => x.Name == "ReservationAccommodation").Relationships.Contains("Bed(BedID, RoomID)"), "dictionary exposes composite bed-room relationship");
        var dictionaryVM = new DataDictionaryViewModel(offlineServices.GetRequiredService<IDataDictionaryService>(), NullLogger<DataDictionaryViewModel>.Instance);
        dictionaryVM.SearchText = "ChargedTaxAmount";
        Check(dictionaryVM.Tables.Count == 1 && dictionaryVM.SelectedTable?.Name == "ReservationTax" && dictionaryVM.Fields.Single().Name == "ChargedTaxAmount", "dictionary searches field names across all tables");
        dictionaryVM.SearchText = "nonexistent-field";
        Check(dictionaryVM.Tables.Count == 0 && dictionaryVM.Fields.Count == 0 && dictionaryVM.SelectedField is null, "dictionary clears stale selection when no search results");
        dictionaryVM.SearchText = "";
        Check(dictionaryVM.Tables.Count == 28, "clearing dictionary search restores full catalog");
    }

    var testName = "Gajyun_Phase34_Verification_" + Guid.NewGuid().ToString("N");
    var testConnection = $"Server=(localdb)\\MSSQLLocalDB;Database={testName};Trusted_Connection=True;TrustServerCertificate=True;";
    Console.WriteLine("Isolated integration database: " + testName);
    var services = new ServiceCollection().AddLogging().AddGajyunData(testConnection).BuildServiceProvider();
    using (services)
    {
        var initialized = await services.GetRequiredService<IDatabaseInitializationService>().InitializeAsync();
        Check(initialized.Succeeded && initialized.ServerConnected, "isolated SQL Server migration and seeding succeed");
        var settings = services.GetRequiredService<IRoomSettingsService>();
        var snapshot = await settings.LoadAsync();
        Check(snapshot.Rooms.Select(x => x.Capacity).SequenceEqual(new short[] { 4, 2, 4, 2, 6 }), "all five physical room capacities exact");
        Check(snapshot.Policies.Count == 2, "2025 and 2026 tax policy options loaded");
        var room = snapshot.Rooms[0];
        var policy = snapshot.Policies[1];
        await settings.SetTaxPolicyAsync(room.RoomID, policy.TaxPolicyID, room.DefaultTaxPolicyID);
        Check((await settings.LoadAsync()).Rooms.Single(x => x.RoomID == room.RoomID).DefaultTaxPolicyID == policy.TaxPolicyID, "room policy preference persists through service");
        try { await settings.SetTaxPolicyAsync(room.RoomID, null, room.DefaultTaxPolicyID); throw new Exception("Stale write accepted"); }
        catch (SettingsOperationException) { Check(true, "stale policy update rejected"); }
        try { await settings.SetTaxPolicyAsync(room.RoomID, int.MaxValue, policy.TaxPolicyID); throw new Exception("Invalid policy accepted"); }
        catch (SettingsOperationException) { Check(true, "unknown policy rejected"); }
        await settings.SetTaxPolicyAsync(room.RoomID, null, policy.TaxPolicyID);
        Check((await settings.LoadAsync()).Rooms.Single(x => x.RoomID == room.RoomID).DefaultTaxPolicyID is null, "room policy preference can be explicitly cleared");
        Check((await settings.GetAliasesAsync(room.RoomID)).Any(x => x.ExternalRoomName.Contains("順")), "room aliases preserve Japanese source names");
        var factory = services.GetRequiredService<IDbContextFactory<GajyunDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        Check(await db.ReservationTaxes.CountAsync() == 0 && await db.ReservationCharges.CountAsync() == 0, "policy changes never create tax snapshots or extra charges");
        Check((await services.GetRequiredService<IDatabaseInitializationService>().InitializeAsync()).Succeeded && await db.Beds.CountAsync() == 14 && await db.OTARoomMappings.CountAsync() == 31, "repeat initialization remains idempotent");
        var picker = new FixedPicker(utf);
        var csvVM = new CsvImportViewModel(service, picker, settings, NullLogger<CsvImportViewModel>.Instance, services.GetRequiredService<IReservationImportService>());
        await csvVM.SelectFileCommand.ExecuteAsync(null);
        Check(csvVM.TotalRows == 2 && csvVM.Preview?.Count == 2 && csvVM.ColumnMappings.Count == 4 && csvVM.HasPreview && !csvVM.CanImport, "CSV ViewModel previews real records with disabled import and mapping foundation");
        csvVM.SelectedEncoding = csvVM.Encodings[2];
        Check(!csvVM.HasPreview && csvVM.Preview is null && csvVM.TotalRows == 0, "changing CSV options invalidates stale preview");
    }
    // Drop only the exact GUID-named database created by this verifier, never the application database.
    await using (var master = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"))
    {
        SqlConnection.ClearAllPools();
        await master.OpenAsync();
        await using var drop = master.CreateCommand();
        drop.CommandText = $"ALTER DATABASE [{testName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{testName}];";
        await drop.ExecuteNonQueryAsync();
    }
    Console.WriteLine($"ALL {checks} CHECKS PASSED");
}
finally
{
    foreach (var file in files) File.Delete(file);
    Directory.Delete(directory);
}

sealed class FixedPicker(string path) : IFileDialogService
{
    public string? SelectCsvFile() => path;
}

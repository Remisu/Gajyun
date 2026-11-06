using System.Globalization;
using System.Text.RegularExpressions;

namespace Gajyun.Core.Csv;

public sealed class CombinedExportReservationParser(NightlyDetailsParser detailsParser, IReservationValidationService validator) : ICsvReservationParser
{
    public string ParserName => "Gajyun combined Japanese export v2 (20261106154030.csv)";
    public static IReadOnlyList<string> RequiredHeaders { get; } = Array.AsReadOnly(new[]
    {
        "予約サイト名", "予約番号", "予約日時", "予約区分", "チェックイン日", "チェックアウト日", "宿泊者氏名",
        "部屋名称", "部屋数", "大人人数", "子供人数", "合計料金", "泊数", "変更日時", "部屋マスタ", "部屋記号", "連泊情報", "部屋料金情報", "詳細"
    });
    public static bool IsKnownSource(string value) => value is "Booking.com" or "Hostelworld" or "Agoda" or "予約情報入力(直接販売)";
    public bool Supports(IReadOnlyList<string> headers) => RequiredHeaders.All(header => headers.Contains(header, StringComparer.Ordinal));
    public IReadOnlyList<string> ValidateMapping(IReadOnlyList<CsvColumnMapping> mappings) =>
        ["This verified combined export uses fixed Japanese source headers and per-row OTA names. Editable generic column mappings are not applied."];

    public ReservationParseResult Parse(IReadOnlyDictionary<string, string> fields)
    {
        if (RequiredHeaders.Any(header => !fields.ContainsKey(header)))
            return new(null, "Required combined-export columns are missing.");
        string Get(string header) => fields[header].Trim();
        var ota = Get("予約サイト名");
        if (!IsKnownSource(ota)) return new(null, "The source OTA has no verified parser. Review is required.");
        if (ota == "Agoda") return new(null, "Agoda held for review: Incl./RefSell. pricing has not been confirmed. No booking or charges will be created.");
        try
        {
            var status = Get("予約区分");
            if (status is not ("予約" or "予約変更" or "キャンセル"))
                return new(null, "Unknown booking status. Review is required.");
            var cancelled = status == "キャンセル";
            var checkIn = ParseDate(Get("チェックイン日"));
            var checkOut = ParseDate(Get("チェックアウト日"));
            if (!short.TryParse(Get("大人人数"), NumberStyles.None, CultureInfo.InvariantCulture, out var adults) ||
                !short.TryParse(Get("子供人数"), NumberStyles.None, CultureInfo.InvariantCulture, out var children) ||
                adults < 0 || children < 0 || adults + children <= 0 || adults + children > short.MaxValue)
                return new(null, "Adult/child counts are invalid or missing.");
            if (!int.TryParse(Get("部屋数"), NumberStyles.None, CultureInfo.InvariantCulture, out var units) || units < 1)
                return new(null, "The source unit count is invalid.");
            if (!int.TryParse(Get("泊数"), NumberStyles.None, CultureInfo.InvariantCulture, out var nightsCount) || nightsCount != checkOut.DayNumber - checkIn.DayNumber)
                return new(null, "The source night count does not match check-in and checkout.");
            var rawDetails = fields["詳細"];
            var parsed = cancelled ? new NightlyDetails([], []) : detailsParser.Parse(rawDetails);
            var nights = parsed.Nights;
            if (!cancelled)
            {
                var schedule = detailsParser.ParseConsecutiveRates(fields["連泊情報"]);
                if (nights.Count == 0)
                {
                    if (ota != "予約情報入力(直接販売)")
                        return new(null, "The nightly details do not contain verified prices.");
                    nights = schedule;
                }
                if (schedule.Count > 0 && (schedule.Count != nights.Count || schedule.Any(rate =>
                        !nights.Any(night => night.ServiceDate == rate.ServiceDate && night.RoomPrice == rate.RoomPrice))))
                    return new(null, "Nightly details and the source nightly schedule disagree. No prices were inferred.");
                var total = NightlyDetailsParser.ParseAmount(Get("合計料金"));
                if (nights.Sum(x => x.RoomPrice) != total)
                    return new(null, "The sum of nightly total-room prices differs from 合計料金. Staff review is required; prices were not multiplied by guest or unit count.");
            }
            var parsedReservation = new ParsedReservation(ota, Get("予約番号"), Get("宿泊者氏名"), Get("部屋名称"), Get("部屋マスタ"),
                checkIn, checkOut, checked((short)(adults + children)), cancelled,
                status == "予約変更", ParseTimestamp(Get("予約日時")), string.IsNullOrEmpty(Get("変更日時")) ? null : ParseTimestamp(Get("変更日時")),
                rawDetails, fields["連泊情報"], fields["部屋料金情報"], nights, parsed.Statements)
            {
                SourcePaymentMethod = detailsParser.HasReportedPrice(rawDetails) ? "Internet payment" : null
            };
            var errors = validator.Validate(parsedReservation);
            return errors.Count == 0 ? new(parsedReservation, cancelled
                ? "Cancelled booking: reservation only, no charges, statements or tax calculations."
                : "Verified nightly room prices; no guest-count multiplication. No payments, bed assignment or automatic tax charges.")
                : new(null, string.Join(" ", errors));
        }
        catch (FormatException exception) { return new(null, exception.Message); }
        catch (RegexMatchTimeoutException) { return new(null, "The nightly details exceeded the parser time limit. Staff review is required."); }
    }

    private static DateOnly ParseDate(string value) => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date : throw new FormatException("A booking date is invalid; expected yyyy-MM-dd.");
    private static DateTime ParseTimestamp(string value) => DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp)
        ? DateTime.SpecifyKind(timestamp, DateTimeKind.Unspecified) : throw new FormatException("A source timestamp is invalid; expected yyyy-MM-dd HH:mm:ss.");
}

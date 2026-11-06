using System.Globalization;
using System.Text.RegularExpressions;

namespace Gajyun.Core.Csv;

public sealed class NightlyDetailsParser
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private const string AmountPattern = @"(?<amount>(?:\d{1,3}(?:,\d{3})+|\d+)(?:\.\d{1,2})?)(?![\d.,])";
    private static readonly Regex Dates = new(@"\[(?<date>\d{4}-\d{2}-\d{2})\](?<body>.*?)(?=\[\d{4}-\d{2}-\d{2}\]|\z)", RegexOptions.Singleline | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex JpyRate = new(@"^\s*(?<plan>(?:(?!JPY).)*?)JPY\s+" + AmountPattern + @"(?:\s+(?<guests>\d+)人)?", RegexOptions.Singleline | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex YenRate = new(@"^\s*(?:(?<plan>.*?)[:：]\s*)?[\\¥￥]\s*" + AmountPattern, RegexOptions.Singleline | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Deposit = new(@"(?<![A-Za-z])deposit\s+JPY\s+" + AmountPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, Timeout);
    private static readonly Regex Commission = new(@"コミッション\(手数料\)[:：]\s*[\\¥￥]\s*" + AmountPattern, RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Schedule = new(@"\[(?<date>\d{4}-\d{2}-\d{2})\]\s*\[[\\¥￥]" + AmountPattern + @"\]", RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex ReportedPrice = new(@"(?:JPY\s+|[\\¥￥]\s*)" + AmountPattern, RegexOptions.CultureInvariant, Timeout);

    public bool HasReportedPrice(string details) => ReportedPrice.IsMatch(details);

    public NightlyDetails Parse(string details)
    {
        ArgumentNullException.ThrowIfNull(details);
        var nights = new List<NightlyRate>();
        foreach (Match entry in Dates.Matches(details))
        {
            if (!DateOnly.TryParseExact(entry.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new FormatException("The nightly details contain an invalid service date.");
            var body = entry.Groups["body"].Value;
            var rate = JpyRate.Match(body);
            if (!rate.Success) rate = YenRate.Match(body);
            if (!rate.Success)
                throw new FormatException("A dated nightly entry has no supported JPY room-price format.");
            int? guests = null;
            if (rate.Groups["guests"].Success)
            {
                if (!int.TryParse(rate.Groups["guests"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count <= 0)
                    throw new FormatException("The nightly entry has an invalid reported guest count.");
                guests = count;
            }
            var plan = rate.Groups["plan"].Value.Trim();
            if (plan.Length > 500) throw new FormatException("The nightly plan exceeds the charge description limit.");
            nights.Add(new NightlyRate(date, plan, ParseAmount(rate.Groups["amount"].Value), guests));
        }
        var statements = new List<SourceStatement>();
        foreach (Match match in Deposit.Matches(details))
            statements.Add(new("deposit", match.Value, ParseAmount(match.Groups["amount"].Value)));
        foreach (Match match in Commission.Matches(details))
            statements.Add(new("コミッション(手数料)", match.Value, ParseAmount(match.Groups["amount"].Value)));
        return new NightlyDetails(nights.AsReadOnly(), statements.AsReadOnly());
    }

    public IReadOnlyList<NightlyRate> ParseConsecutiveRates(string text)
    {
        var nights = new List<NightlyRate>();
        foreach (Match match in Schedule.Matches(text))
        {
            if (!DateOnly.TryParseExact(match.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new FormatException("The nightly rate schedule contains an invalid date.");
            nights.Add(new(date, "Original nightly room price (breakfast-inclusive plan; taxable base unverified)", ParseAmount(match.Groups["amount"].Value), null));
        }
        return nights.AsReadOnly();
    }

    public static decimal ParseAmount(string value)
    {
        if (!Regex.IsMatch(value, @"\A(?:\d{1,3}(?:,\d{3})+|\d+)(?:\.\d{1,2})?\z", RegexOptions.CultureInvariant, Timeout) ||
            !decimal.TryParse(value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var amount) || amount < 0 || amount > 9_999_999_999.99m || decimal.Round(amount, 2) != amount)
            throw new FormatException("A source amount is not a non-negative decimal(12,2) JPY value.");
        return amount;
    }
}

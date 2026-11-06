using System.Globalization;
using Gajyun.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data.Seeding;

internal static class LookupSeedData
{
    public const string CombinedLogoUrl = "pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/VariousLogo.png";
    private const string TaxSource = "Verified Kyoto City rates; staff approved 2026-11-06. Meals and consumption tax excluded. https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html";

    public static void Configure(ModelBuilder model)
    {
        model.Entity<OTA>().HasData(
            new OTA { OTAID = 1, OTAName = "Booking.com", LogoUrl = Logo("Booking-Logo.png") },
            new OTA { OTAID = 2, OTAName = "Agoda", LogoUrl = Logo("Agoda-logo.png") },
            new OTA { OTAID = 3, OTAName = "Hostelworld", LogoUrl = Logo("hostelworld-seeklogo.png") },
            new OTA { OTAID = 4, OTAName = "予約情報入力(直接販売)", LogoUrl = Logo("gajyun-logo.png") });
        model.Entity<DocumentType>().HasData(
            new DocumentType { DocumentTypeID = 1, DocumentTypeName = "Passport" },
            new DocumentType { DocumentTypeID = 2, DocumentTypeName = "Driver's license" },
            new DocumentType { DocumentTypeID = 3, DocumentTypeName = "Foreign residence card" },
            new DocumentType { DocumentTypeID = 4, DocumentTypeName = "Other" });
        model.Entity<PaymentMethod>().HasData(
            new PaymentMethod { PaymentMethodID = 1, MethodName = "Cash" },
            new PaymentMethod { PaymentMethodID = 2, MethodName = "Credit Card" },
            new PaymentMethod { PaymentMethodID = 3, MethodName = "Bank Transfer" },
            new PaymentMethod { PaymentMethodID = 4, MethodName = "Other" },
            new PaymentMethod { PaymentMethodID = 5, MethodName = "Internet payment" });
        model.Entity<TaxPolicy>().HasData(
            new TaxPolicy { TaxPolicyID = 1, PolicyName = "Kyoto City Accommodation Tax 2025", ValidFrom = new(2018, 10, 1), ValidTo = new(2026, 3, 1), Notes = TaxSource },
            new TaxPolicy { TaxPolicyID = 2, PolicyName = "Kyoto City Accommodation Tax 2026", ValidFrom = new(2026, 3, 1), Notes = TaxSource });
        model.Entity<TaxBracket>().HasData(
            Bracket(1, 1, 0m, 20000m, 200m), Bracket(2, 1, 20000m, 50000m, 500m), Bracket(3, 1, 50000m, null, 1000m),
            Bracket(4, 2, 0m, 6000m, 200m), Bracket(5, 2, 6000m, 20000m, 400m), Bracket(6, 2, 20000m, 50000m, 1000m),
            Bracket(7, 2, 50000m, 100000m, 4000m), Bracket(8, 2, 100000m, null, 10000m));

        var specialNames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AQ"] = "Antarctica", ["BV"] = "Bouvet Island", ["EH"] = "Western Sahara",
            ["GS"] = "South Georgia and the South Sandwich Islands", ["HM"] = "Heard Island and McDonald Islands",
            ["TF"] = "French Southern Territories", ["XK"] = "Kosovo (user-assigned XK code)"
        };
        var resources = typeof(LookupSeedData).Assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith("Gajyun.Flags.", StringComparison.Ordinal) && x.EndsWith(".png", StringComparison.Ordinal))
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (resources.Length != 250) throw new InvalidOperationException("The supplied flag catalog must contain 250 two-letter assets.");
        var countries = resources.Select((resource, index) =>
        {
            var code = resource["Gajyun.Flags.".Length..^4].ToUpperInvariant();
            return new Country
            {
                CountryID = index + 1, ISO2Code = code,
                CountryName = specialNames.GetValueOrDefault(code) ?? new RegionInfo(code).EnglishName,
                FlagUrl = $"pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/{code.ToLowerInvariant()}.png"
            };
        }).ToArray();
        model.Entity<Country>().HasData(countries);
    }

    private static string Logo(string name) => $"pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/{name}";
    private static TaxBracket Bracket(int id, int policy, decimal min, decimal? max, decimal amount) =>
        new() { TaxBracketID = id, TaxPolicyID = policy, MinPriceInclusive = min, MaxPriceExclusive = max, TaxPerPersonNight = amount };
}

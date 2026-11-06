namespace Gajyun.Core.Models;

public sealed class OTA
{
    public int OTAID { get; set; }
    public required string OTAName { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class BookingStatus
{
    public int BookingStatusID { get; set; }
    public required string StatusName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Country
{
    public int CountryID { get; set; }
    public required string CountryName { get; set; }
    public string? ISO2Code { get; set; }
    public string? FlagUrl { get; set; }
}

public sealed class Room
{
    public int RoomID { get; set; }
    public required string RoomName { get; set; }
    public short Capacity { get; set; }
    public int? DefaultTaxPolicyID { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Bed
{
    public int BedID { get; set; }
    public int RoomID { get; set; }
    public required string BedLabel { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PaymentMethod
{
    public int PaymentMethodID { get; set; }
    public required string MethodName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class DocumentType
{
    public int DocumentTypeID { get; set; }
    public required string DocumentTypeName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ExtraService
{
    public int ExtraServiceID { get; set; }
    public required string ServiceName { get; set; }
    public decimal DefaultUnitPrice { get; set; }
    public string CurrencyCode { get; set; } = "JPY";
    public bool IsActive { get; set; } = true;
}

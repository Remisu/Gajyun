namespace Gajyun.Core.Models;

public sealed class ReservationCharge
{
    public long ChargeID { get; set; }
    public long ReservationID { get; set; }
    public int? ExtraServiceID { get; set; }
    public required string ChargeType { get; set; }
    public DateOnly? ServiceDate { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; set; } = "JPY";
    public string? SourceLineKey { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ReservationPayment
{
    public long PaymentID { get; set; }
    public long ReservationID { get; set; }
    public int? PaymentMethodID { get; set; }
    public required string Direction { get; set; }
    public required string CounterpartyType { get; set; }
    public required string PaymentKind { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "JPY";
    public DateTime? PaidAt { get; set; }
    public string? ExternalReference { get; set; }
    public string? Notes { get; set; }
}

public sealed class OTAStatementItem
{
    public long OTAStatementItemID { get; set; }
    public long ReservationID { get; set; }
    public required string RawLabel { get; set; }
    public string? RawText { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "JPY";
    public string Classification { get; set; } = "Unclassified";
    public string? ResolutionNotes { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? SourceLineKey { get; set; }
}

public sealed class PaymentAllocation
{
    public long PaymentID { get; set; }
    public long ChargeID { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public sealed class TaxPolicy
{
    public int TaxPolicyID { get; set; }
    public required string PolicyName { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class TaxBracket
{
    public int TaxBracketID { get; set; }
    public int TaxPolicyID { get; set; }
    public decimal MinPriceInclusive { get; set; }
    public decimal? MaxPriceExclusive { get; set; }
    public decimal TaxPerPersonNight { get; set; }
}

public sealed class ReservationTax
{
    public long ReservationTaxID { get; set; }
    public long ReservationID { get; set; }
    public DateOnly ServiceDate { get; set; }
    public int? RoomID { get; set; }
    public int? LegalTaxPolicyID { get; set; }
    public int? AppliedTaxPolicyID { get; set; }
    public short? GuestCount { get; set; }
    public decimal? TaxablePricePerPerson { get; set; }
    public decimal? LegalTaxAmount { get; set; }
    public decimal? ChargedTaxAmount { get; set; }
    public string CalculationStatus { get; set; } = "PendingReview";
    public string? Notes { get; set; }
    public DateTime? CalculatedAt { get; set; }
}

public sealed class ReservationFinancialSummary
{
    public long ReservationID { get; set; }
    public required string CurrencyCode { get; set; }
    public decimal TotalCharged { get; set; }
    public decimal NetCashMovement { get; set; }
    public decimal UnreconciledDifference { get; set; }
}

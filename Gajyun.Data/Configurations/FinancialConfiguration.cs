using Gajyun.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data.Configurations;

internal static class FinancialConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var charge = model.Entity<ReservationCharge>();
        charge.HasKey(x => x.ChargeID);
        charge.Property(x => x.ChargeType).HasMaxLength(30).IsRequired();
        charge.Property(x => x.Description).HasMaxLength(500);
        charge.Property(x => x.Quantity).HasPrecision(10, 2).HasDefaultValue(1m, "DF_ReservationCharge_Quantity");
        charge.Property(x => x.Amount).HasPrecision(18, 2).HasComputedColumnSql("CONVERT(DECIMAL(18,2), [Quantity] * [UnitPrice])", stored: true);
        charge.Property(x => x.CurrencyCode).HasDefaultValue("JPY", "DF_ReservationCharge_Currency");
        charge.Property(x => x.SourceLineKey).HasMaxLength(150);
        charge.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_ReservationCharge_Created");
        charge.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ReservationCharge_Reservation");
        charge.HasOne<ExtraService>().WithMany().HasForeignKey(x => x.ExtraServiceID).HasConstraintName("FK_ReservationCharge_Extra");
        charge.HasIndex(x => new { x.ReservationID, x.SourceLineKey }).IsUnique().HasFilter("[SourceLineKey] IS NOT NULL")
            .HasDatabaseName("UX_ReservationCharge_SourceLine");
        charge.HasIndex(x => x.ReservationID).HasDatabaseName("IX_ReservationCharge_Reservation");
        charge.ToTable("ReservationCharge", t =>
        {
            t.HasCheckConstraint("CK_ReservationCharge_Quantity", "[Quantity] > 0");
            t.HasCheckConstraint("CK_ReservationCharge_Type", "[ChargeType] IN ('Accommodation','Breakfast','Laundry','Towel','LateCheckIn','Tax','Discount','Adjustment','Other')");
            t.HasCheckConstraint("CK_ReservationCharge_SignedPrice", "[ChargeType] IN ('Discount','Adjustment') OR [UnitPrice] >= 0");
        });

        var payment = model.Entity<ReservationPayment>();
        payment.HasKey(x => x.PaymentID);
        payment.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        payment.Property(x => x.CounterpartyType).HasMaxLength(20).IsRequired();
        payment.Property(x => x.PaymentKind).HasMaxLength(25).IsRequired();
        payment.Property(x => x.CurrencyCode).HasDefaultValue("JPY", "DF_ReservationPayment_Currency");
        payment.Property(x => x.ExternalReference).HasMaxLength(150);
        payment.Property(x => x.Notes).HasMaxLength(500);
        payment.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ReservationPayment_Reservation");
        payment.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.PaymentMethodID).HasConstraintName("FK_ReservationPayment_Method");
        payment.HasIndex(x => x.ReservationID).HasDatabaseName("IX_ReservationPayment_Reservation");
        payment.ToTable("ReservationPayment", t =>
        {
            t.HasCheckConstraint("CK_ReservationPayment_Direction", "[Direction] IN ('Inflow','Outflow')");
            t.HasCheckConstraint("CK_ReservationPayment_Party", "[CounterpartyType] IN ('Guest','OTA','Guesthouse','Other')");
            t.HasCheckConstraint("CK_ReservationPayment_Kind", "[PaymentKind] IN ('Payment','Refund','Payout','Other')");
            t.HasCheckConstraint("CK_ReservationPayment_Amount", "[Amount] > 0");
        });

        var statement = model.Entity<OTAStatementItem>();
        statement.HasKey(x => x.OTAStatementItemID);
        statement.Property(x => x.RawLabel).HasMaxLength(100).IsRequired();
        statement.Property(x => x.RawText).HasMaxLength(500);
        statement.Property(x => x.CurrencyCode).HasDefaultValue("JPY", "DF_OTAStatementItem_Currency");
        statement.Property(x => x.Classification).HasMaxLength(25).IsRequired().HasDefaultValue("Unclassified", "DF_OTAStatementItem_Class");
        statement.Property(x => x.ResolutionNotes).HasMaxLength(500);
        statement.Property(x => x.SourceLineKey).HasMaxLength(150);
        statement.Property(x => x.ImportedAt).HasDefaultValueSql("SYSDATETIME()", "DF_OTAStatementItem_Imported");
        statement.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_OTAStatementItem_Reservation");
        statement.HasIndex(x => new { x.ReservationID, x.SourceLineKey }).IsUnique().HasFilter("[SourceLineKey] IS NOT NULL")
            .HasDatabaseName("UX_OTAStatementItem_SourceLine");
        statement.ToTable("OTAStatementItem", t =>
        {
            t.HasCheckConstraint("CK_OTAStatementItem_Classification", "[Classification] IN ('Unclassified','Commission','GuestDeposit','Payout','TaxWithheld','Other')");
            t.HasCheckConstraint("CK_OTAStatementItem_Amount", "[Amount] >= 0");
        });

        var allocation = model.Entity<PaymentAllocation>();
        allocation.HasKey(x => new { x.PaymentID, x.ChargeID }).HasName("PK_PaymentAllocation");
        allocation.HasOne<ReservationPayment>().WithMany().HasForeignKey(x => x.PaymentID).HasConstraintName("FK_PaymentAllocation_Payment");
        allocation.HasOne<ReservationCharge>().WithMany().HasForeignKey(x => x.ChargeID).HasConstraintName("FK_PaymentAllocation_Charge");
        allocation.ToTable("PaymentAllocation", t => t.HasCheckConstraint("CK_PaymentAllocation_Positive", "[AllocatedAmount] > 0"));

        var summary = model.Entity<ReservationFinancialSummary>();
        summary.HasNoKey().ToView("vwReservationFinancialSummary", "dbo");
        summary.Property(x => x.TotalCharged).HasPrecision(38, 2);
        summary.Property(x => x.NetCashMovement).HasPrecision(38, 2);
        summary.Property(x => x.UnreconciledDifference).HasPrecision(38, 2);
    }
}

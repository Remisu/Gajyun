using Gajyun.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data.Configurations;

internal static class TaxAndImportConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var policy = model.Entity<TaxPolicy>();
        policy.HasKey(x => x.TaxPolicyID);
        policy.Property(x => x.PolicyName).HasMaxLength(160).IsRequired();
        policy.Property(x => x.Notes).HasMaxLength(1000);
        policy.Property(x => x.IsActive).HasDefaultValue(true, "DF_TaxPolicy_Active");
        policy.HasIndex(x => x.PolicyName).IsUnique();
        policy.ToTable("TaxPolicy", t => t.HasCheckConstraint("CK_TaxPolicy_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));

        var bracket = model.Entity<TaxBracket>();
        bracket.HasKey(x => x.TaxBracketID);
        bracket.HasOne<TaxPolicy>().WithMany().HasForeignKey(x => x.TaxPolicyID).HasConstraintName("FK_TaxBracket_TaxPolicy");
        bracket.HasIndex(x => new { x.TaxPolicyID, x.MinPriceInclusive }).IsUnique();
        bracket.ToTable("TaxBracket", t =>
        {
            t.HasCheckConstraint("CK_TaxBracket_Range", "[MinPriceInclusive] >= 0 AND ([MaxPriceExclusive] IS NULL OR [MaxPriceExclusive] > [MinPriceInclusive])");
            t.HasCheckConstraint("CK_TaxBracket_Amount", "[TaxPerPersonNight] >= 0");
        });

        var tax = model.Entity<ReservationTax>();
        tax.HasKey(x => x.ReservationTaxID);
        tax.Property(x => x.CalculationStatus).HasMaxLength(24).IsRequired().HasDefaultValue("PendingReview", "DF_ReservationTax_Status");
        tax.Property(x => x.Notes).HasMaxLength(1000);
        tax.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ReservationTax_Reservation");
        tax.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomID).HasConstraintName("FK_ReservationTax_Room");
        tax.HasOne<TaxPolicy>().WithMany().HasForeignKey(x => x.LegalTaxPolicyID).HasConstraintName("FK_ReservationTax_LegalPolicy");
        tax.HasOne<TaxPolicy>().WithMany().HasForeignKey(x => x.AppliedTaxPolicyID).HasConstraintName("FK_ReservationTax_AppliedPolicy");
        tax.HasIndex(x => new { x.ReservationID, x.ServiceDate });
        tax.ToTable("ReservationTax", t =>
        {
            t.HasCheckConstraint("CK_ReservationTax_Status", "[CalculationStatus] IN ('PendingReview','Calculated','Confirmed','NotApplicable')");
            t.HasCheckConstraint("CK_ReservationTax_Guests", "[GuestCount] IS NULL OR [GuestCount] > 0");
            t.HasCheckConstraint("CK_ReservationTax_Amounts", "([TaxablePricePerPerson] IS NULL OR [TaxablePricePerPerson] >= 0) AND ([LegalTaxAmount] IS NULL OR [LegalTaxAmount] >= 0) AND ([ChargedTaxAmount] IS NULL OR [ChargedTaxAmount] >= 0)");
        });

        var batch = model.Entity<ImportBatch>();
        batch.HasKey(x => x.ImportBatchID);
        batch.Property(x => x.SourceFileName).HasMaxLength(255);
        batch.Property(x => x.ImportedAt).HasDefaultValueSql("SYSDATETIME()", "DF_ImportBatch_Imported");
        batch.Property(x => x.SourceFileHash).HasColumnType("varchar(64)");
        batch.Property(x => x.SourceEncoding).HasMaxLength(100);
        batch.Property(x => x.SourceFileContent).HasColumnType("varbinary(max)");
        batch.Property(x => x.ParserVersion).HasMaxLength(40);
        batch.HasOne<OTA>().WithMany().HasForeignKey(x => x.OTAID).HasConstraintName("FK_ImportBatch_OTA");

        var row = model.Entity<ImportRow>();
        row.HasKey(x => x.ImportRowID);
        row.Property(x => x.RawPayload).IsRequired();
        row.Property(x => x.ParseStatus).HasMaxLength(20).IsRequired().HasDefaultValue("Pending", "DF_ImportRow_Status");
        row.Property(x => x.ParseError).HasMaxLength(2000);
        row.Property(x => x.SourceRowHash).HasColumnType("varchar(64)");
        row.HasIndex(x => new { x.ReservationID, x.SourceRowHash });
        row.HasOne<ImportBatch>().WithMany().HasForeignKey(x => x.ImportBatchID).HasConstraintName("FK_ImportRow_Batch");
        row.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ImportRow_Reservation");
        row.HasAlternateKey(x => new { x.ImportBatchID, x.SourceRowNumber }).HasName("UQ_ImportRow_BatchNumber");
        row.ToTable("ImportRow", t => t.HasCheckConstraint("CK_ImportRow_Status", "[ParseStatus] IN ('Pending','Processed','Failed','Skipped')"));

        var mapping = model.Entity<OTARoomMapping>();
        mapping.HasKey(x => x.OTARoomMappingID);
        mapping.Property(x => x.ExternalRoomName).HasMaxLength(255).IsRequired();
        mapping.Property(x => x.NormalizedName).HasMaxLength(255).IsRequired().UseCollation("Latin1_General_100_BIN2");
        mapping.Property(x => x.IsActive).HasDefaultValue(true, "DF_OTARoomMapping_Active");
        mapping.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_OTARoomMapping_Created");
        mapping.HasOne<OTA>().WithMany().HasForeignKey(x => x.OTAID).HasConstraintName("FK_OTARoomMapping_OTA");
        mapping.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomID).HasConstraintName("FK_OTARoomMapping_Room");
        mapping.HasIndex(x => new { x.OTAID, x.NormalizedName }).IsUnique()
            .HasFilter("[OTAID] IS NOT NULL AND [IsActive] = 1").HasDatabaseName("UX_OTARoomMapping_OTAName");
        mapping.HasIndex(x => new { x.RoomID, x.NormalizedName }).IsUnique()
            .HasFilter("[OTAID] IS NULL AND [IsActive] = 1").HasDatabaseName("UX_OTARoomMapping_GenericRoomName");
        mapping.HasIndex(x => x.NormalizedName).HasDatabaseName("IX_OTARoomMapping_NormalizedName");
    }
}

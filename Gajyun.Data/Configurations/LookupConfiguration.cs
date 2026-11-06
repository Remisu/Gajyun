using Gajyun.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data.Configurations;

internal static class LookupConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var ota = model.Entity<OTA>();
        ota.HasKey(x => x.OTAID);
        ota.Property(x => x.OTAName).HasMaxLength(120).IsRequired();
        ota.Property(x => x.LogoUrl).HasMaxLength(500);
        ota.Property(x => x.IsActive).HasDefaultValue(true, "DF_OTA_Active");
        ota.HasIndex(x => x.OTAName).IsUnique();

        var status = model.Entity<BookingStatus>();
        status.HasKey(x => x.BookingStatusID);
        status.Property(x => x.StatusName).HasMaxLength(60).IsRequired();
        status.Property(x => x.IsActive).HasDefaultValue(true, "DF_BookingStatus_Active");
        status.HasIndex(x => x.StatusName).IsUnique();

        var country = model.Entity<Country>();
        country.HasKey(x => x.CountryID);
        country.Property(x => x.CountryName).HasMaxLength(120).IsRequired();
        country.Property(x => x.ISO2Code).HasColumnType("char(2)");
        country.Property(x => x.FlagUrl).HasMaxLength(500);
        country.HasIndex(x => x.ISO2Code).IsUnique().HasFilter("[ISO2Code] IS NOT NULL")
            .HasDatabaseName("UX_Country_ISO2Code");

        var room = model.Entity<Room>();
        room.HasKey(x => x.RoomID);
        room.Property(x => x.RoomName).HasMaxLength(100).IsRequired();
        room.Property(x => x.IsActive).HasDefaultValue(true, "DF_Room_Active");
        room.HasIndex(x => x.RoomName).IsUnique();
        room.HasOne<TaxPolicy>().WithMany().HasForeignKey(x => x.DefaultTaxPolicyID)
            .HasConstraintName("FK_Room_DefaultTaxPolicy");
        room.ToTable("Room", t => t.HasCheckConstraint("CK_Room_Capacity", "[Capacity] > 0"));

        var bed = model.Entity<Bed>();
        bed.HasKey(x => x.BedID);
        bed.Property(x => x.BedLabel).HasMaxLength(80).IsRequired();
        bed.Property(x => x.IsActive).HasDefaultValue(true, "DF_Bed_Active");
        bed.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomID).HasConstraintName("FK_Bed_Room");
        bed.HasAlternateKey(x => new { x.RoomID, x.BedLabel }).HasName("UQ_Bed_Room_Label");
        bed.HasAlternateKey(x => new { x.BedID, x.RoomID }).HasName("UQ_Bed_BedRoom");

        var method = model.Entity<PaymentMethod>();
        method.HasKey(x => x.PaymentMethodID);
        method.Property(x => x.MethodName).HasMaxLength(80).IsRequired();
        method.Property(x => x.IsActive).HasDefaultValue(true, "DF_PaymentMethod_Active");
        method.HasIndex(x => x.MethodName).IsUnique();

        var document = model.Entity<DocumentType>();
        document.HasKey(x => x.DocumentTypeID);
        document.Property(x => x.DocumentTypeName).HasMaxLength(100).IsRequired();
        document.Property(x => x.IsActive).HasDefaultValue(true, "DF_DocumentType_Active");
        document.HasIndex(x => x.DocumentTypeName).IsUnique();

        var extra = model.Entity<ExtraService>();
        extra.HasKey(x => x.ExtraServiceID);
        extra.Property(x => x.ServiceName).HasMaxLength(100).IsRequired();
        extra.HasIndex(x => x.ServiceName).IsUnique();
        extra.Property(x => x.DefaultUnitPrice).HasDefaultValue(0m, "DF_ExtraService_Price");
        extra.Property(x => x.CurrencyCode).HasDefaultValue("JPY", "DF_ExtraService_Currency");
        extra.Property(x => x.IsActive).HasDefaultValue(true, "DF_ExtraService_Active");
        extra.ToTable("ExtraService", t => t.HasCheckConstraint("CK_ExtraService_Price", "[DefaultUnitPrice] >= 0"));
    }
}

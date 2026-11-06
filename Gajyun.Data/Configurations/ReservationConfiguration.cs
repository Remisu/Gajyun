using Gajyun.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gajyun.Data.Configurations;

internal static class ReservationConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var group = model.Entity<ReservationGroup>();
        group.HasKey(x => x.GroupID);
        group.Property(x => x.ExternalGroupReference).HasMaxLength(120);
        group.Property(x => x.LogoUrl).HasMaxLength(500);
        group.Property(x => x.Notes).HasMaxLength(1000);

        var customer = model.Entity<Customer>();
        customer.HasKey(x => x.CustomerID);
        customer.Property(x => x.FullName).HasMaxLength(255).IsRequired();
        customer.Property(x => x.Nickname).HasMaxLength(255);
        customer.Property(x => x.PostalAddress).HasMaxLength(500);
        customer.Property(x => x.PhoneNumber).HasMaxLength(50);
        customer.Property(x => x.Email).HasMaxLength(254);
        customer.Property(x => x.IsRegularCustomer).HasDefaultValue(false, "DF_Customer_Regular");
        customer.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_Customer_Created");
        customer.HasOne<Country>().WithMany().HasForeignKey(x => x.CountryID).HasConstraintName("FK_Customer_Country");

        var reservation = model.Entity<Reservation>();
        reservation.HasKey(x => x.ReservationID);
        reservation.Property(x => x.ExternalReservationNumber).HasMaxLength(120);
        reservation.Property(x => x.CheckInStatus).HasMaxLength(20).IsRequired().HasDefaultValue("Pending", "DF_Reservation_CheckInStatus");
        reservation.Property(x => x.IsActive).HasDefaultValue(true, "DF_Reservation_Active");
        reservation.Property(x => x.IsModified).HasDefaultValue(false, "DF_Reservation_Modified");
        reservation.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_Reservation_Created");
        reservation.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_Reservation_Updated");
        reservation.HasOne<OTA>().WithMany().HasForeignKey(x => x.OTAID).HasConstraintName("FK_Reservation_OTA");
        reservation.HasOne<ReservationGroup>().WithMany().HasForeignKey(x => x.GroupID).HasConstraintName("FK_Reservation_Group");
        reservation.HasOne<BookingStatus>().WithMany().HasForeignKey(x => x.BookingStatusID).HasConstraintName("FK_Reservation_BookingStatus");
        reservation.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.SourcePaymentMethodID).HasConstraintName("FK_Reservation_SourcePaymentMethod");
        reservation.HasIndex(x => x.BookedAt).HasDatabaseName("IX_Reservation_BookedAt");
        reservation.HasIndex(x => new { x.OTAID, x.ExternalReservationNumber }).IsUnique()
            .HasFilter("[ExternalReservationNumber] IS NOT NULL").HasDatabaseName("UX_Reservation_OTA_ExternalNumber");
        reservation.HasIndex(x => x.CheckInDate).HasDatabaseName("IX_Reservation_CheckInDate");
        reservation.HasIndex(x => x.CheckOutDate).HasDatabaseName("IX_Reservation_CheckOutDate");
        reservation.ToTable("Reservation", t =>
        {
            t.HasCheckConstraint("CK_Reservation_Dates", "[CheckOutDate] > [CheckInDate]");
            t.HasCheckConstraint("CK_Reservation_Guests", "[ExpectedGuestCount] IS NULL OR [ExpectedGuestCount] > 0");
            t.HasCheckConstraint("CK_Reservation_CheckInStatus", "[CheckInStatus] IN ('Pending','CheckedIn','NoShow')");
            t.HasCheckConstraint("CK_Reservation_CheckInTime", "([CheckInStatus] = 'CheckedIn' AND [ActualCheckInAt] IS NOT NULL) OR ([CheckInStatus] <> 'CheckedIn' AND [ActualCheckInAt] IS NULL)");
        });

        var guest = model.Entity<ReservationCustomer>();
        guest.HasKey(x => new { x.ReservationID, x.CustomerID }).HasName("PK_ReservationCustomer");
        guest.Property(x => x.IsLeadGuest).HasDefaultValue(false, "DF_ReservationCustomer_Lead");
        guest.Property(x => x.AddedAt).HasDefaultValueSql("SYSDATETIME()", "DF_ReservationCustomer_Added");
        guest.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ReservationCustomer_Reservation");
        guest.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerID).HasConstraintName("FK_ReservationCustomer_Customer");
        guest.HasIndex(x => x.ReservationID).IsUnique().HasFilter("[IsLeadGuest] = 1").HasDatabaseName("UX_ReservationCustomer_OneLead");

        var accommodation = model.Entity<ReservationAccommodation>();
        accommodation.HasKey(x => x.ReservationAccommodationID);
        accommodation.Property(x => x.Notes).HasMaxLength(300);
        accommodation.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_ReservationAccommodation_Reservation");
        accommodation.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomID).HasConstraintName("FK_ReservationAccommodation_Room");
        accommodation.HasOne<Bed>().WithMany().HasForeignKey(x => new { x.BedID, x.RoomID })
            .HasPrincipalKey(x => new { x.BedID, x.RoomID }).HasConstraintName("FK_ReservationAccommodation_BedRoom");
        accommodation.HasIndex(x => new { x.RoomID, x.BedID, x.StartDate, x.EndDate }).HasDatabaseName("IX_ReservationAccommodation_Inventory");
        accommodation.ToTable("ReservationAccommodation", t => t.HasCheckConstraint("CK_ReservationAccommodation_Dates", "[EndDate] > [StartDate]"));

        var memo = model.Entity<StayMemo>();
        memo.HasKey(x => x.MemoID);
        memo.Property(x => x.MemoText).IsRequired();
        memo.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_StayMemo_Created");
        memo.HasOne<Stay>().WithMany().HasForeignKey(x => x.StayID).HasConstraintName("FK_StayMemo_Stay");

        var document = model.Entity<CustomerDocument>();
        document.HasKey(x => x.DocumentID);
        document.Property(x => x.FileStorageKey).HasMaxLength(1000);
        document.Property(x => x.PhotoStorageKey).HasMaxLength(1000);
        document.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_CustomerDocument_Created");
        document.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerID).HasConstraintName("FK_CustomerDocument_Customer");
        document.HasOne<DocumentType>().WithMany().HasForeignKey(x => x.DocumentTypeID).HasConstraintName("FK_CustomerDocument_Type");

        var stay = model.Entity<Stay>();
        stay.HasKey(x => x.StayID);
        stay.Property(x => x.Notes).HasMaxLength(1000);
        stay.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()", "DF_Stay_Created");
        stay.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerID).HasConstraintName("FK_Stay_Customer");
        stay.ToTable("Stay", t => t.HasCheckConstraint("CK_Stay_Dates", "[EndDate] > [StartDate]"));

        var link = model.Entity<StayReservation>();
        link.HasKey(x => new { x.StayID, x.ReservationID });
        link.Property(x => x.AddedAt).HasDefaultValueSql("SYSDATETIME()", "DF_StayReservation_Added");
        link.HasOne<Stay>().WithMany().HasForeignKey(x => x.StayID).HasConstraintName("FK_StayReservation_Stay");
        link.HasOne<Reservation>().WithMany().HasForeignKey(x => x.ReservationID).HasConstraintName("FK_StayReservation_Reservation");
    }
}

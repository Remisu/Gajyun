namespace Gajyun.Core.Models;

public sealed class ReservationGroup
{
    public long GroupID { get; set; }
    public string? ExternalGroupReference { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
}

public sealed class Customer
{
    public long CustomerID { get; set; }
    public required string FullName { get; set; }
    public string? Nickname { get; set; }
    public int? CountryID { get; set; }
    public string? PostalAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsRegularCustomer { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class Reservation
{
    public long ReservationID { get; set; }
    public int OTAID { get; set; }
    public string? ExternalReservationNumber { get; set; }
    public long? GroupID { get; set; }
    public int BookingStatusID { get; set; }
    public int? SourcePaymentMethodID { get; set; }
    public DateTime? BookedAt { get; set; }
    public DateTime? ModifiedAtSource { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public short? ExpectedGuestCount { get; set; }
    public DateTime? ActualCheckInAt { get; set; }
    public string CheckInStatus { get; set; } = "Pending";
    public bool IsActive { get; set; } = true;
    public bool IsModified { get; set; }
    public string? ConsecutiveStayInfo { get; set; }
    public string? RoomRateInfo { get; set; }
    public string? BookingDetails { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class ReservationCustomer
{
    public long ReservationID { get; set; }
    public long CustomerID { get; set; }
    public bool IsLeadGuest { get; set; }
    public DateTime AddedAt { get; set; }
}

public sealed class ReservationAccommodation
{
    public long ReservationAccommodationID { get; set; }
    public long ReservationID { get; set; }
    public int RoomID { get; set; }
    public int? BedID { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class StayMemo
{
    public long MemoID { get; set; }
    public long StayID { get; set; }
    public required string MemoText { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CustomerDocument
{
    public long DocumentID { get; set; }
    public long CustomerID { get; set; }
    public int DocumentTypeID { get; set; }
    public string? FileStorageKey { get; set; }
    public string? PhotoStorageKey { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class Stay
{
    public long StayID { get; set; }
    public long CustomerID { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class StayReservation
{
    public long StayID { get; set; }
    public long ReservationID { get; set; }
    public DateTime AddedAt { get; set; }
}

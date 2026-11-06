namespace Gajyun.Core.Csv;

public sealed class ReservationValidationService : IReservationValidationService
{
    public IReadOnlyList<string> Validate(ParsedReservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(reservation.ExternalNumber) || reservation.ExternalNumber.Length > 120)
            errors.Add("The OTA booking identifier must contain 1-120 characters.");
        if (string.IsNullOrWhiteSpace(reservation.GuestName) || reservation.GuestName.Length > 255)
            errors.Add("The supplied guest name is missing or exceeds 255 characters.");
        if (reservation.CheckOut <= reservation.CheckIn) errors.Add("Checkout must be after check-in.");
        if (reservation.GuestCount <= 0) errors.Add("The expected guest count must be positive.");
        if (string.IsNullOrWhiteSpace(reservation.ExternalRoomName) || reservation.ExternalRoomName.Length > 255 || reservation.RoomMaster.Length > 255)
            errors.Add("The source room name is missing or exceeds its supported length.");
        if (!reservation.IsCancelled)
        {
            var expectedNights = reservation.CheckOut.DayNumber - reservation.CheckIn.DayNumber;
            if (reservation.Nights.Count != expectedNights || reservation.Nights.Select(x => x.ServiceDate).Distinct().Count() != expectedNights ||
                reservation.Nights.Any(x => x.ServiceDate < reservation.CheckIn || x.ServiceDate >= reservation.CheckOut))
                errors.Add("A unique nightly price is required for every night in the booking interval.");
            if (reservation.Nights.Any(x => x.RoomPrice < 0 || x.RoomPrice > 9_999_999_999.99m || x.Description.Length > 500))
                errors.Add("A nightly rate or description exceeds the charge schema limits.");
        }
        return errors.AsReadOnly();
    }
}

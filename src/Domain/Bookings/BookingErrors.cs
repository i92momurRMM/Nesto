using Nesto.SharedKernel;

namespace Nesto.Domain.Bookings;

public static class BookingErrors
{
    public static Error NotFound(Guid bookingId) => Error.NotFound(
        "Bookings.NotFound",
        $"The booking with identifier {bookingId} was not found.");

    public static readonly Error NotReserved = Error.Conflict(
        "Bookings.NotReserved",
        "Only a reserved booking can be confirmed or rejected.");

    public static readonly Error NotConfirmed = Error.Conflict(
        "Bookings.NotConfirmed",
        "Only a confirmed booking can be completed or cancelled.");

    public static readonly Error AlreadyStarted = Error.Conflict(
        "Bookings.AlreadyStarted",
        "A booking cannot be cancelled after the stay has started.");

    public static readonly Error NotCompleted = Error.Conflict(
        "Bookings.NotCompleted",
        "The stay has not ended yet.");
}

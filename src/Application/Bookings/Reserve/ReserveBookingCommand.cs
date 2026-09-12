using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Reserve;

public sealed record ReserveBookingCommand(
    Guid ApartmentId,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand<Guid>;

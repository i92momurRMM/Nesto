using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Cancel;

public sealed record CancelBookingCommand(Guid BookingId) : ICommand;

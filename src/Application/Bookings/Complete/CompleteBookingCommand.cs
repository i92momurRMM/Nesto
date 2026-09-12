using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Complete;

public sealed record CompleteBookingCommand(Guid BookingId) : ICommand;

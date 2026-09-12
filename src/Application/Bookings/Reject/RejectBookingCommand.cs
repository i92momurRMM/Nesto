using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Reject;

public sealed record RejectBookingCommand(Guid BookingId) : ICommand;

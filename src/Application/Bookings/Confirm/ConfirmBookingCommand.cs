using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Confirm;

public sealed record ConfirmBookingCommand(Guid BookingId) : ICommand;

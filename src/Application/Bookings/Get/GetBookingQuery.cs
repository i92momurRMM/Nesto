using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Bookings.Get;

public sealed record GetBookingQuery(Guid BookingId) : IQuery<BookingResponse>;

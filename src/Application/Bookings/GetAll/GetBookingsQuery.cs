using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Get;

namespace Nesto.Application.Bookings.GetAll;

public sealed record GetBookingsQuery : IQuery<IReadOnlyList<BookingResponse>>;

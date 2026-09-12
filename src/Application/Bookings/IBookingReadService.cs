using Nesto.Application.Bookings.Get;

namespace Nesto.Application.Bookings;

public interface IBookingReadService
{
    Task<BookingResponse?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

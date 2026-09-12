using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Bookings;

internal sealed class BookingRepository(ApplicationDbContext context) : IBookingRepository
{
    private static readonly BookingStatus[] ActiveStatuses =
    [
        BookingStatus.Reserved,
        BookingStatus.Confirmed,
        BookingStatus.Completed
    ];

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Bookings.SingleOrDefaultAsync(booking => booking.Id == id, cancellationToken);

    public Task<bool> IsOverlappingAsync(
        Apartment apartment,
        DateRange duration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(apartment);
        ArgumentNullException.ThrowIfNull(duration);

        return context.Bookings.AnyAsync(
            booking =>
                booking.ApartmentId == apartment.Id &&
                booking.Duration.Start < duration.End &&
                booking.Duration.End > duration.Start &&
                ActiveStatuses.Contains(booking.Status),
            cancellationToken);
    }

    public void Add(Booking booking) => context.Bookings.Add(booking);
}

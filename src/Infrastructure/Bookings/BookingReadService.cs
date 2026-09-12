using Nesto.Application.Bookings;
using Nesto.Application.Bookings.Get;
using Nesto.Domain.Bookings;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Bookings;

internal sealed class BookingReadService(ApplicationDbContext context) : IBookingReadService
{
    public async Task<BookingResponse?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        Booking? booking = await context.Bookings
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == bookingId, cancellationToken);

        return booking is null ? null : Map(booking);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<Booking> bookings = await context.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        return [.. bookings.Select(Map)];
    }

    private static BookingResponse Map(Booking booking) =>
        new()
        {
            Id = booking.Id,
            ApartmentId = booking.ApartmentId,
            UserId = booking.UserId,
            Status = (int)booking.Status,
            PriceAmount = booking.PriceForPeriod.Amount,
            PriceCurrency = booking.PriceForPeriod.Currency.Code,
            CleaningFeeAmount = booking.CleaningFee.Amount,
            AmenitiesUpChargeAmount = booking.AmenitiesUpCharge.Amount,
            TotalPriceAmount = booking.TotalPrice.Amount,
            DurationStart = booking.Duration.Start,
            DurationEnd = booking.Duration.End,
            CreatedOnUtc = booking.CreatedOnUtc
        };
}

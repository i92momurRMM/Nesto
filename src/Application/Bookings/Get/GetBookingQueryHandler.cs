using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings;
using Nesto.Domain.Bookings;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.Get;

internal sealed class GetBookingQueryHandler(
    IBookingReadService readService,
    IUserContext userContext)
    : IQueryHandler<GetBookingQuery, BookingResponse>
{
    public async Task<Result<BookingResponse>> Handle(GetBookingQuery query, CancellationToken cancellationToken)
    {
        BookingResponse? booking = await readService.GetByIdAsync(query.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure<BookingResponse>(BookingErrors.NotFound(query.BookingId));
        }

        if (booking.UserId != userContext.UserId)
        {
            return Result.Failure<BookingResponse>(UserErrors.Unauthorized());
        }

        return booking;
    }
}

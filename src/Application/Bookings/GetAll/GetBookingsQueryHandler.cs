using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings;
using Nesto.Application.Bookings.Get;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.GetAll;

internal sealed class GetBookingsQueryHandler(
    IBookingReadService readService,
    IUserContext userContext) : IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingResponse>>
{
    public async Task<Result<IReadOnlyList<BookingResponse>>> Handle(
        GetBookingsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingResponse> bookings = await readService.GetByUserIdAsync(
            userContext.UserId,
            cancellationToken);
        return Result.Success(bookings);
    }
}

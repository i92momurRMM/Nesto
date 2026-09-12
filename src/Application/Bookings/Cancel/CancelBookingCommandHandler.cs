using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Bookings;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.Cancel;

internal sealed class CancelBookingCommandHandler(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    ICacheService cache) : ICommandHandler<CancelBookingCommand>
{
    public async Task<Result> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(command.BookingId));
        }

        if (booking.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Result result = booking.Cancel(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Apartments, cancellationToken);

        return Result.Success();
    }
}

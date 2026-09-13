using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Bookings;
using Nesto.Domain.Apartments;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.Reject;

internal sealed class RejectBookingCommandHandler(
    IBookingRepository repository,
    IApartmentRepository apartmentRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IDateTimeProvider clock,
    ICacheService cache) : ICommandHandler<RejectBookingCommand>
{
    public async Task<Result> Handle(RejectBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await repository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(command.BookingId));
        }

        Apartment? apartment = await apartmentRepository.GetByIdAsync(booking.ApartmentId, cancellationToken);
        if (apartment?.OwnerId != userContext.UserId)
        {
            return Result.Failure(BookingErrors.NotApartmentOwner);
        }

        Result result = booking.Reject(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Apartments, cancellationToken);
        return Result.Success();
    }
}

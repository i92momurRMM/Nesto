using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.Complete;

internal sealed class CompleteBookingCommandHandler(
    IBookingRepository repository,
    IApartmentRepository apartmentRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IDateTimeProvider clock) : ICommandHandler<CompleteBookingCommand>
{
    public async Task<Result> Handle(CompleteBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await repository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(command.BookingId));
        }

        Apartment? apartment = await apartmentRepository.GetByIdAsync(booking.ApartmentId, cancellationToken);
        if (apartment?.OwnerId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Result result = booking.Complete(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

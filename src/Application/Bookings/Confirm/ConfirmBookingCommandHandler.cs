using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.SharedKernel;
using Nesto.Domain.Users;

namespace Nesto.Application.Bookings.Confirm;

internal sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository,
    IApartmentRepository apartmentRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ConfirmBookingCommand>
{
    public async Task<Result> Handle(ConfirmBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(command.BookingId));
        }

        Apartment? apartment = await apartmentRepository.GetByIdAsync(booking.ApartmentId, cancellationToken);

        if (apartment?.OwnerId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Result result = booking.Confirm(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

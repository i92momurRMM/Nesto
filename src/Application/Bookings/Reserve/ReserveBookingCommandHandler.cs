using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings.Reserve;

internal sealed class ReserveBookingCommandHandler(
    IApartmentRepository apartmentRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    PricingService pricingService,
    IDateTimeProvider dateTimeProvider,
    ICacheService cache) : ICommandHandler<ReserveBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ReserveBookingCommand command, CancellationToken cancellationToken)
    {
        Apartment? apartment = await apartmentRepository.GetByIdAsync(command.ApartmentId, cancellationToken);

        if (apartment is null)
        {
            return Result.Failure<Guid>(ApartmentErrors.NotFound(command.ApartmentId));
        }

        var duration = DateRange.Create(command.StartDate, command.EndDate);

        if (await bookingRepository.IsOverlappingAsync(apartment, duration, cancellationToken))
        {
            return Result.Failure<Guid>(ApartmentErrors.NotAvailable);
        }

        try
        {
            var booking = Booking.Reserve(
                apartment,
                userContext.UserId,
                duration,
                dateTimeProvider.UtcNow,
                pricingService);

            bookingRepository.Add(booking);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync(CacheTags.Apartments, cancellationToken);

            return booking.Id;
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<Guid>(ApartmentErrors.NotAvailable);
        }
    }
}

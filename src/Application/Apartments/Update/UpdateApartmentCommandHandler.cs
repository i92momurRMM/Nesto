using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Apartments.Update;

internal sealed class UpdateApartmentCommandHandler(
    IApartmentRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    ICacheService cache) : ICommandHandler<UpdateApartmentCommand>
{
    public async Task<Result> Handle(UpdateApartmentCommand command, CancellationToken cancellationToken)
    {
        Apartment? apartment = await repository.GetByIdAsync(command.ApartmentId, cancellationToken);

        if (apartment is null)
        {
            return Result.Failure(ApartmentErrors.NotFound(command.ApartmentId));
        }

        if (apartment.OwnerId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        apartment.Update(
            new Money(command.PriceAmount, Currency.FromCode(command.PriceCurrency)),
            new Money(command.CleaningFeeAmount, Currency.FromCode(command.CleaningFeeCurrency)),
            command.Amenities);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Apartments, cancellationToken);
        return Result.Success();
    }
}

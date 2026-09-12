using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.SharedKernel;

namespace Nesto.Application.Apartments.Create;

internal sealed class CreateApartmentCommandHandler(
    IApartmentRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    ICacheService cache) : ICommandHandler<CreateApartmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateApartmentCommand command, CancellationToken cancellationToken)
    {
        var apartment = Apartment.Create(
            userContext.UserId,
            Name.Create(command.Name),
            Description.Create(command.Description),
            new Address(command.Country, command.State, command.ZipCode, command.City, command.Street),
            new Money(command.PriceAmount, Currency.FromCode(command.PriceCurrency)),
            new Money(command.CleaningFeeAmount, Currency.FromCode(command.CleaningFeeCurrency)),
            command.Amenities);

        repository.Add(apartment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync(CacheTags.Apartments, cancellationToken);
        return apartment.Id;
    }
}

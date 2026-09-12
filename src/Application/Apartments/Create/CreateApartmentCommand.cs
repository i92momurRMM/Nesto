using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;

namespace Nesto.Application.Apartments.Create;

public sealed record CreateApartmentCommand(
    string Name,
    string Description,
    string Country,
    string State,
    string ZipCode,
    string City,
    string Street,
    decimal PriceAmount,
    string PriceCurrency,
    decimal CleaningFeeAmount,
    string CleaningFeeCurrency,
    Amenity[] Amenities) : ICommand<Guid>;

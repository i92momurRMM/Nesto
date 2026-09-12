using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Apartments;

namespace Nesto.Application.Apartments.Update;

public sealed record UpdateApartmentCommand(
    Guid ApartmentId,
    decimal PriceAmount,
    string PriceCurrency,
    decimal CleaningFeeAmount,
    string CleaningFeeCurrency,
    Amenity[] Amenities) : ICommand;

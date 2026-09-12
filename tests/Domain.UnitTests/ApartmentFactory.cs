using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;

namespace Nesto.Domain.UnitTests;

internal static class ApartmentFactory
{
    public static Apartment Create(
        decimal price = 100m,
        decimal cleaningFee = 0m,
        params Amenity[] amenities) =>
        Apartment.Create(
            Name.Create("Atico en Malasaña"),
            Description.Create("Dos habitaciones, terraza y mucha luz."),
            new Address("España", "Madrid", "28004", "Madrid", "Calle de la Palma 1"),
            new Money(price, Currency.Eur),
            new Money(cleaningFee, Currency.Eur),
            amenities);
}

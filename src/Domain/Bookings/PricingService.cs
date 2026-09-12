using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;

namespace Nesto.Domain.Bookings;

public sealed class PricingService
{
    public PricingDetails CalculatePrice(Apartment apartment, DateRange period)
    {
        ArgumentNullException.ThrowIfNull(apartment);
        ArgumentNullException.ThrowIfNull(period);

        Currency currency = apartment.Price.Currency;

        var priceForPeriod = new Money(apartment.Price.Amount * period.LengthInDays, currency);

        decimal percentageUpCharge = apartment.Amenities.Sum(UpChargePercentageFor);

        var amenitiesUpCharge = Money.Zero(currency);
        if (percentageUpCharge > 0)
        {
            amenitiesUpCharge = new Money(
                priceForPeriod.Amount * (percentageUpCharge / 100),
                currency);
        }

        Money totalPrice = Money.Zero(currency)
            .Add(priceForPeriod)
            .Add(apartment.CleaningFee.IsZero() ? Money.Zero(currency) : apartment.CleaningFee)
            .Add(amenitiesUpCharge);

        return new PricingDetails(priceForPeriod, apartment.CleaningFee, amenitiesUpCharge, totalPrice);
    }

    private static decimal UpChargePercentageFor(Amenity amenity) => amenity switch
    {
        Amenity.GardenView or Amenity.MountainView => 5m,
        Amenity.AirConditioning or Amenity.Parking => 1m,
        _ => 0m
    };
}

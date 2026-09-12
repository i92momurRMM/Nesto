using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;

namespace Nesto.Domain.UnitTests.Bookings;

public class PricingServiceTests
{
    private static readonly DateRange TwoNights = DateRange.Create(
        new DateOnly(2026, 9, 1),
        new DateOnly(2026, 9, 3));

    private readonly PricingService _pricingService = new();

    [Fact]
    public void CalculatePrice_MultiplicaElPrecioPorNoche_PorElNumeroDeNoches()
    {
        Apartment apartment = ApartmentFactory.Create(price: 100m);

        PricingDetails details = _pricingService.CalculatePrice(apartment, TwoNights);

        details.PriceForPeriod.Amount.ShouldBe(200m);
        details.TotalPrice.Amount.ShouldBe(200m);
    }

    [Fact]
    public void CalculatePrice_SumaLaTasaDeLimpieza_UnaSolaVezPorEstancia()
    {
        Apartment apartment = ApartmentFactory.Create(price: 100m, cleaningFee: 35m);

        PricingDetails details = _pricingService.CalculatePrice(apartment, TwoNights);

        details.TotalPrice.Amount.ShouldBe(235m);
    }

    [Fact]
    public void CalculatePrice_AplicaElRecargoDeLosServicios_SobreElPrecioDelPeriodo()
    {
        Apartment apartment = ApartmentFactory.Create(
            price: 100m,
            cleaningFee: 0m,
            Amenity.GardenView,      // 5%
            Amenity.AirConditioning); // 1%

        PricingDetails details = _pricingService.CalculatePrice(apartment, TwoNights);

        details.AmenitiesUpCharge.Amount.ShouldBe(12m); // 6% of 200
        details.TotalPrice.Amount.ShouldBe(212m);
    }

    [Fact]
    public void CalculatePrice_MantieneLaDivisaDelApartamento()
    {
        Apartment apartment = ApartmentFactory.Create(price: 100m, cleaningFee: 35m);

        PricingDetails details = _pricingService.CalculatePrice(apartment, TwoNights);

        details.TotalPrice.Currency.ShouldBe(Currency.Eur);
    }
}

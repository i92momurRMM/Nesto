using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;
using Nesto.SharedKernel;

namespace Nesto.Domain.UnitTests.Bookings;

public class BookingTests
{
    private static readonly DateTime Now = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);

    private static readonly DateRange Duration = DateRange.Create(
        new DateOnly(2026, 9, 10),
        new DateOnly(2026, 9, 12));

    private readonly PricingService _pricingService = new();

    [Fact]
    public void Reserve_DejaLaReservaEnEstadoReserved_YSellaElPrecio()
    {
        Apartment apartment = ApartmentFactory.Create(price: 100m, cleaningFee: 20m);

        var booking = Booking.Reserve(apartment, Guid.NewGuid(), Duration, Now, _pricingService);

        booking.Status.ShouldBe(BookingStatus.Reserved);
        booking.TotalPrice.Amount.ShouldBe(220m);
        booking.CreatedOnUtc.ShouldBe(Now);
    }

    [Fact]
    public void Reserve_LevantaElEventoDeDominio()
    {
        Apartment apartment = ApartmentFactory.Create();

        var booking = Booking.Reserve(apartment, Guid.NewGuid(), Duration, Now, _pricingService);

        IDomainEvent domainEvent = booking.DomainEvents.ShouldHaveSingleItem();
        domainEvent.ShouldBeOfType<BookingReservedDomainEvent>()
            .BookingId.ShouldBe(booking.Id);
    }

    [Fact]
    public void Reserve_MarcaElApartamentoComoReservado()
    {
        Apartment apartment = ApartmentFactory.Create();

        Booking.Reserve(apartment, Guid.NewGuid(), Duration, Now, _pricingService);

        apartment.LastBookedOnUtc.ShouldBe(Now);
    }

    [Fact]
    public void Confirm_DesdeReserved_Funciona()
    {
        Booking booking = Reserved();

        Result result = booking.Confirm(Now);

        result.IsSuccess.ShouldBeTrue();
        booking.Status.ShouldBe(BookingStatus.Confirmed);
        booking.ConfirmedOnUtc.ShouldBe(Now);
    }

    [Fact]
    public void Confirm_DosVeces_Falla()
    {
        Booking booking = Reserved();
        booking.Confirm(Now);

        Result result = booking.Confirm(Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BookingErrors.NotReserved);
    }

    [Fact]
    public void Cancel_AntesDeLaFechaDeEntrada_Funciona()
    {
        Booking booking = Reserved();
        booking.Confirm(Now);

        Result result = booking.Cancel(Now);

        result.IsSuccess.ShouldBeTrue();
        booking.Status.ShouldBe(BookingStatus.Cancelled);
    }

    [Fact]
    public void Cancel_UnaVezEmpezadaLaEstancia_Falla()
    {
        Booking booking = Reserved();
        booking.Confirm(Now);

        var duranteLaEstancia = new DateTime(2026, 9, 11, 9, 0, 0, DateTimeKind.Utc);

        Result result = booking.Cancel(duranteLaEstancia);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BookingErrors.AlreadyStarted);
        booking.Status.ShouldBe(BookingStatus.Confirmed);
    }

    [Fact]
    public void Cancel_SinConfirmar_Falla()
    {
        Booking booking = Reserved();

        Result result = booking.Cancel(Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BookingErrors.NotConfirmed);
    }

    [Fact]
    public void Reject_DesdeReserved_Funciona()
    {
        Booking booking = Reserved();

        Result result = booking.Reject(Now);

        result.IsSuccess.ShouldBeTrue();
        booking.Status.ShouldBe(BookingStatus.Rejected);
    }

    [Fact]
    public void Complete_SinConfirmar_Falla()
    {
        Booking booking = Reserved();

        Result result = booking.Complete(Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(BookingErrors.NotConfirmed);
    }

    private Booking Reserved() =>
        Booking.Reserve(ApartmentFactory.Create(), Guid.NewGuid(), Duration, Now, _pricingService);
}

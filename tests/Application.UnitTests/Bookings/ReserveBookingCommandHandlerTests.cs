using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Caching;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Bookings.Reserve;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;
using Nesto.SharedKernel;

namespace Nesto.Application.UnitTests.Bookings;

public class ReserveBookingCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IApartmentRepository _apartmentRepository = Substitute.For<IApartmentRepository>();
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    private readonly ReserveBookingCommand _command = new(
        Guid.NewGuid(),
        new DateOnly(2026, 9, 10),
        new DateOnly(2026, 9, 12));

    public ReserveBookingCommandHandlerTests()
    {
        _userContext.UserId.Returns(UserId);
        _dateTimeProvider.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Handle_ConApartamentoInexistente_DevuelveNotFound()
    {
        _apartmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Apartment?)null);

        Result<Guid> result = await CreateHandler().Handle(_command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Apartments.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConFechasOcupadas_DevuelveConflicto()
    {
        GivenApartmentExists();
        _bookingRepository.IsOverlappingAsync(
                Arg.Any<Apartment>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Result<Guid> result = await CreateHandler().Handle(_command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ApartmentErrors.NotAvailable);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConApartamentoLibre_CreaLaReserva()
    {
        GivenApartmentExists();
        _bookingRepository.IsOverlappingAsync(
                Arg.Any<Apartment>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(false);

        Result<Guid> result = await CreateHandler().Handle(_command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _bookingRepository.Received(1).Add(Arg.Is<Booking>(b => b.UserId == UserId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoOtraTransaccionGanaLaCarrera_DevuelveConflicto()
    {
        GivenApartmentExists();
        _bookingRepository.IsOverlappingAsync(
                Arg.Any<Apartment>(), Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new ConcurrencyException("colision"));

        Result<Guid> result = await CreateHandler().Handle(_command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ApartmentErrors.NotAvailable);
    }

    private void GivenApartmentExists() =>
        _apartmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Apartment.Create(
                Name.Create("Atico en Malasaña"),
                Description.Create("Dos habitaciones y terraza."),
                new Address("España", "Madrid", "28004", "Madrid", "Calle de la Palma 1"),
                new Money(100m, Currency.Eur),
                new Money(20m, Currency.Eur),
                []));

    private ReserveBookingCommandHandler CreateHandler() =>
        new(_apartmentRepository,
            _bookingRepository,
            _unitOfWork,
            _userContext,
            new PricingService(),
            _dateTimeProvider,
            _cache);
}

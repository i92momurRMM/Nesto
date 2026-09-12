using Nesto.Domain.Shared;
using Nesto.Domain.Users;

namespace Nesto.Domain.UnitTests.Shared;

public class ValueObjectTests
{
    [Fact]
    public void DateRange_ConSalidaAntesDeLaEntrada_NoSePuedeCrear()
    {
        Should.Throw<ArgumentException>(() =>
            DateRange.Create(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void DateRange_ConLaMismaFechaDeEntradaYSalida_NoSePuedeCrear()
    {
        var sameDay = new DateOnly(2026, 9, 10);

        Should.Throw<ArgumentException>(() => DateRange.Create(sameDay, sameDay));
    }

    [Fact]
    public void DateRange_CuentaNoches_NoDias()
    {
        var range = DateRange.Create(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 12));

        range.LengthInDays.ShouldBe(2);
    }

    [Fact]
    public void Money_SumarDivisasDistintas_Falla()
    {
        var euros = new Money(10m, Currency.Eur);
        var dolares = new Money(10m, Currency.Usd);

        Should.Throw<InvalidOperationException>(() => euros.Add(dolares));
    }

    [Fact]
    public void Money_SumarSobreZero_AdoptaLaDivisaDelSumando()
    {
        Money total = Money.Zero().Add(new Money(10m, Currency.Eur));

        total.Currency.ShouldBe(Currency.Eur);
        total.Amount.ShouldBe(10m);
    }

    [Theory]
    [InlineData("no-es-un-email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("dos@arrobas@ejemplo.com")]
    public void Email_ConValorInvalido_TryCreateDevuelveNull(string value)
    {
        Email.TryCreate(value).ShouldBeNull();
    }

    [Fact]
    public void Email_ConValorValido_SeCrea()
    {
        var email = Email.TryCreate("rafael@ejemplo.com");

        email.ShouldNotBeNull();
        email.Value.ShouldBe("rafael@ejemplo.com");
    }

    [Fact]
    public void Currency_DesconocidaFalla()
    {
        Should.Throw<InvalidOperationException>(() => Currency.FromCode("XXX"));
    }
}

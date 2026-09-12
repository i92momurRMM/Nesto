namespace Nesto.Domain.Shared;

public sealed record Money(decimal Amount, Currency Currency)
{
    public static Money Zero() => new(0, Currency.None);

    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);

    public static Money operator +(Money first, Money second) => first.Add(second);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency && !Currency.IsNone && !other.Currency.IsNone)
        {
            throw new InvalidOperationException("Money values with different currencies cannot be added.");
        }

        return new Money(Amount + other.Amount, Currency.IsNone ? other.Currency : Currency);
    }
}

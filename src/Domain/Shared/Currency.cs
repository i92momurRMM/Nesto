namespace Nesto.Domain.Shared;

public sealed record Currency
{
    internal static readonly Currency None = new(string.Empty);

    public static readonly Currency Eur = new("EUR");
    public static readonly Currency Usd = new("USD");

    private Currency(string code) => Code = code;

    public string Code { get; init; }

    public bool IsNone => Code.Length == 0;

    public static IReadOnlyCollection<Currency> All => [Eur, Usd];

    public static Currency FromCode(string code) =>
        All.FirstOrDefault(c => c.Code == code) ??
        throw new InvalidOperationException($"Currency '{code}' is not supported.");
}

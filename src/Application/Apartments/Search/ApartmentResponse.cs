namespace Nesto.Application.Apartments.Search;

public sealed record ApartmentResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public decimal PriceAmount { get; init; }

    public string PriceCurrency { get; init; }

    public string Country { get; init; }

    public string City { get; init; }

    public string Street { get; init; }
}

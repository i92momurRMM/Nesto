namespace Nesto.Domain.Shared;

public sealed record Address(
    string Country,
    string State,
    string ZipCode,
    string City,
    string Street);

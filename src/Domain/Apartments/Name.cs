namespace Nesto.Domain.Apartments;

public sealed record Name
{
    public const int MaxLength = 200;

    private Name(string value) => Value = value;

    public string Value { get; init; }

    public static Name Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentException(
                $"The name cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        return new Name(value);
    }
}

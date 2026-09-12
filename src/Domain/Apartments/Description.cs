namespace Nesto.Domain.Apartments;

public sealed record Description
{
    public const int MaxLength = 2000;

    private Description(string value) => Value = value;

    public string Value { get; init; }

    public static Description Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentException(
                $"The description cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        return new Description(value);
    }
}

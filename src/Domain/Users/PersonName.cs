namespace Nesto.Domain.Users;

public sealed record PersonName
{
    public const int MaxLength = 100;

    private PersonName(string value) => Value = value;

    public string Value { get; init; }

    public static PersonName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentException(
                $"The name cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        return new PersonName(value);
    }
}

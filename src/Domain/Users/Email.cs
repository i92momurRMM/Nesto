using System.Net.Mail;

namespace Nesto.Domain.Users;

public sealed record Email
{
    public const int MaxLength = 320;

    private Email(string value) => Value = value;

    public string Value { get; init; }

    public static Email Create(string value) =>
        TryCreate(value) ?? throw new ArgumentException($"'{value}' no es un email valido.", nameof(value));

    public static Email? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
        {
            return null;
        }

        return MailAddress.TryCreate(value, out MailAddress? address) && address.Address == value
            ? new Email(value)
            : null;
    }
}

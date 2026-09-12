namespace Nesto.Domain.Users;

public sealed class RefreshToken
{
    private readonly User _user = null!;

    private RefreshToken()
    {
    }

    private RefreshToken(Guid id, string token, Guid userId, DateTime expiresOnUtc)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public Guid Id { get; private set; }

    public string Token { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    public User User => _user;

    internal static RefreshToken Issue(string token, Guid userId, DateTime expiresOnUtc) =>
        new(Guid.CreateVersion7(), token, userId, expiresOnUtc);

    public bool IsExpired(DateTime utcNow) => ExpiresOnUtc <= utcNow;
}

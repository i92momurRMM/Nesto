namespace Nesto.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveRefreshTokenAsync(
        string token,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    void Add(User user);

    void AddRefreshToken(RefreshToken refreshToken);

    void RemoveRefreshToken(RefreshToken refreshToken);
}

using Nesto.Domain.Users;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Users;

internal sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<bool> ExistsWithEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<RefreshToken?> GetActiveRefreshTokenAsync(
        string token,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        context.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .SingleOrDefaultAsync(
                refreshToken => refreshToken.Token == token && refreshToken.ExpiresOnUtc > utcNow,
                cancellationToken);

    public void Add(User user) => context.Users.Add(user);

    public void AddRefreshToken(RefreshToken refreshToken) => context.RefreshTokens.Add(refreshToken);

    public void RemoveRefreshToken(RefreshToken refreshToken) => context.RefreshTokens.Remove(refreshToken);
}

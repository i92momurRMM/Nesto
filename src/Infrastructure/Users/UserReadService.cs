using Nesto.Application.Users;
using Nesto.Domain.Users;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Users;

internal sealed class UserReadService(ApplicationDbContext context) : IUserReadService
{
    public async Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        return user is null ? null : Map(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var userEmail = Email.Create(email);
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Email == userEmail, cancellationToken);

        return user is null ? null : Map(user);
    }

    private static UserResponse Map(User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName.Value,
            LastName = user.LastName.Value
        };
}

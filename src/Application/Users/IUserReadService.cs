namespace Nesto.Application.Users;

public interface IUserReadService
{
    Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}

using Nesto.SharedKernel;

namespace Nesto.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<Result> CreateAsync(Guid userId, string email, string password, CancellationToken cancellationToken);

    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);
}

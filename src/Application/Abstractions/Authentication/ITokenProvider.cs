using Nesto.Domain.Users;

namespace Nesto.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);

    string GenerateRefreshToken();
}

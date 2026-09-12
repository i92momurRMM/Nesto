using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AccessTokensResponse>;

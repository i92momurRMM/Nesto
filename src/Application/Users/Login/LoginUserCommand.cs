using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AccessTokensResponse>;

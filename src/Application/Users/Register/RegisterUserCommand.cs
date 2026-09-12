using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
    : ICommand<Guid>;

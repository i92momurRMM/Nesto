using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.GetByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponse>;

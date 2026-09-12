using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;

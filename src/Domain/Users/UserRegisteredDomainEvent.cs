using Nesto.SharedKernel;

namespace Nesto.Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;

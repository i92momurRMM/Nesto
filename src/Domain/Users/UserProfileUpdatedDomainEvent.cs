using Nesto.SharedKernel;

namespace Nesto.Domain.Users;

public sealed record UserProfileUpdatedDomainEvent(Guid UserId) : IDomainEvent;

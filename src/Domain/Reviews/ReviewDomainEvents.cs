using Nesto.SharedKernel;

namespace Nesto.Domain.Reviews;

public sealed record ReviewCreatedDomainEvent(Guid ReviewId) : IDomainEvent;

public sealed record ReviewUpdatedDomainEvent(Guid ReviewId) : IDomainEvent;

public sealed record ReviewDeletedDomainEvent(Guid ReviewId) : IDomainEvent;

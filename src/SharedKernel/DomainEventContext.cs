namespace Nesto.SharedKernel;

public sealed record DomainEventContext(Guid EventId, DateTime OccurredOnUtc);

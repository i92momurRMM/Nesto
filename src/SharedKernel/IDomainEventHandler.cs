namespace Nesto.SharedKernel;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(
        T domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken);
}

using Nesto.SharedKernel;

namespace Nesto.Infrastructure.DomainEvents;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(
        IDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken = default);
}

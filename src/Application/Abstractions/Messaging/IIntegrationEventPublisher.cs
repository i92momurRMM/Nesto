using Nesto.Contracts.IntegrationEvents;

namespace Nesto.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

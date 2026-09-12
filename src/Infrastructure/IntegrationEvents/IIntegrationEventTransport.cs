using Nesto.Contracts.IntegrationEvents;

namespace Nesto.Infrastructure.IntegrationEvents;

internal interface IIntegrationEventTransport
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}

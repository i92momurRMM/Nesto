using Nesto.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Nesto.Infrastructure.IntegrationEvents;

internal sealed class LoggingIntegrationEventTransport(
    ILogger<LoggingIntegrationEventTransport> logger) : IIntegrationEventTransport
{
    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Published integration event {IntegrationEventType} with id {IntegrationEventId}",
            integrationEvent.GetType().Name,
            integrationEvent.Id);

        return Task.CompletedTask;
    }
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.IntegrationEvents;

internal sealed class OutboxIntegrationEventPublisher(ApplicationDbContext context)
    : IIntegrationEventPublisher
{
    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        bool isTracked = context.IntegrationOutboxMessages.Local.Any(
            message => message.Id == integrationEvent.Id);

        if (isTracked || await context.IntegrationOutboxMessages.AnyAsync(
                message => message.Id == integrationEvent.Id,
                cancellationToken))
        {
            return;
        }

        context.IntegrationOutboxMessages.Add(IntegrationOutboxMessage.Create(integrationEvent));
    }
}

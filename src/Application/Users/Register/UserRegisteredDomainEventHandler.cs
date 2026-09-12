using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.Register;

internal sealed class UserRegisteredDomainEventHandler(
    IIntegrationEventPublisher publisher)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public Task Handle(
        UserRegisteredDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken)
    {
        return publisher.PublishAsync(
            new UserRegisteredIntegrationEvent(
                IntegrationEventId.From<UserRegisteredIntegrationEvent>(context),
                context.OccurredOnUtc,
                domainEvent.UserId),
            cancellationToken);
    }
}

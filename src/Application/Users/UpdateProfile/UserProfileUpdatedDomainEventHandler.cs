using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Users.UpdateProfile;

internal sealed class UserProfileUpdatedDomainEventHandler(
    IIntegrationEventPublisher publisher)
    : IDomainEventHandler<UserProfileUpdatedDomainEvent>
{
    public Task Handle(
        UserProfileUpdatedDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        publisher.PublishAsync(
            new UserProfileUpdatedIntegrationEvent(
                IntegrationEventId.From<UserProfileUpdatedIntegrationEvent>(context),
                context.OccurredOnUtc,
                domainEvent.UserId),
            cancellationToken);
}

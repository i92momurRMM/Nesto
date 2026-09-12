using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Reviews;
using Nesto.SharedKernel;

namespace Nesto.Application.Reviews;

internal sealed class ReviewDomainEventHandlers(
    IIntegrationEventPublisher publisher) :
    IDomainEventHandler<ReviewCreatedDomainEvent>,
    IDomainEventHandler<ReviewUpdatedDomainEvent>,
    IDomainEventHandler<ReviewDeletedDomainEvent>
{
    public Task Handle(ReviewCreatedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new ReviewCreatedIntegrationEvent(
            IntegrationEventId.From<ReviewCreatedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.ReviewId), cancellationToken);

    public Task Handle(ReviewUpdatedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new ReviewUpdatedIntegrationEvent(
            IntegrationEventId.From<ReviewUpdatedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.ReviewId), cancellationToken);

    public Task Handle(ReviewDeletedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new ReviewDeletedIntegrationEvent(
            IntegrationEventId.From<ReviewDeletedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.ReviewId), cancellationToken);

    private Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        publisher.PublishAsync(integrationEvent, cancellationToken);
}

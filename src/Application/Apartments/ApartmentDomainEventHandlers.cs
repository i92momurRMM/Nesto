using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Apartments;
using Nesto.SharedKernel;

namespace Nesto.Application.Apartments;

internal sealed class ApartmentDomainEventHandlers(
    IIntegrationEventPublisher publisher) :
    IDomainEventHandler<ApartmentCreatedDomainEvent>,
    IDomainEventHandler<ApartmentUpdatedDomainEvent>
{
    public Task Handle(
        ApartmentCreatedDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        publisher.PublishAsync(
            new ApartmentCreatedIntegrationEvent(
                IntegrationEventId.From<ApartmentCreatedIntegrationEvent>(context),
                context.OccurredOnUtc,
                domainEvent.ApartmentId,
                domainEvent.OwnerId),
            cancellationToken);

    public Task Handle(
        ApartmentUpdatedDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken) =>
        publisher.PublishAsync(
            new ApartmentUpdatedIntegrationEvent(
                IntegrationEventId.From<ApartmentUpdatedIntegrationEvent>(context),
                context.OccurredOnUtc,
                domainEvent.ApartmentId,
                domainEvent.OwnerId),
            cancellationToken);
}

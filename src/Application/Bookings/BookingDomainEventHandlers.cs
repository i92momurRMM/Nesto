using Nesto.Application.Abstractions.Messaging;
using Nesto.Contracts.IntegrationEvents;
using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Bookings;
using Nesto.SharedKernel;

namespace Nesto.Application.Bookings;

internal sealed class BookingDomainEventHandlers(
    IIntegrationEventPublisher publisher) :
    IDomainEventHandler<BookingReservedDomainEvent>,
    IDomainEventHandler<BookingConfirmedDomainEvent>,
    IDomainEventHandler<BookingRejectedDomainEvent>,
    IDomainEventHandler<BookingCompletedDomainEvent>,
    IDomainEventHandler<BookingCancelledDomainEvent>
{
    public Task Handle(BookingReservedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new BookingReservedIntegrationEvent(
            IntegrationEventId.From<BookingReservedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.BookingId), cancellationToken);

    public Task Handle(BookingConfirmedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new BookingConfirmedIntegrationEvent(
            IntegrationEventId.From<BookingConfirmedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.BookingId), cancellationToken);

    public Task Handle(BookingRejectedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new BookingRejectedIntegrationEvent(
            IntegrationEventId.From<BookingRejectedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.BookingId), cancellationToken);

    public Task Handle(BookingCompletedDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new BookingCompletedIntegrationEvent(
            IntegrationEventId.From<BookingCompletedIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.BookingId), cancellationToken);

    public Task Handle(BookingCancelledDomainEvent domainEvent, DomainEventContext context, CancellationToken cancellationToken) =>
        PublishAsync(new BookingCancelledIntegrationEvent(
            IntegrationEventId.From<BookingCancelledIntegrationEvent>(context),
            context.OccurredOnUtc,
            domainEvent.BookingId), cancellationToken);

    private Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        publisher.PublishAsync(integrationEvent, cancellationToken);
}

namespace Nesto.Contracts.IntegrationEvents.V1;

public sealed record ApartmentCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid ApartmentId,
    Guid OwnerId) : IIntegrationEvent;

public sealed record ApartmentUpdatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid ApartmentId,
    Guid OwnerId) : IIntegrationEvent;

namespace Nesto.Contracts.IntegrationEvents.V1;

public sealed record ReviewCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid ReviewId) : IIntegrationEvent;

public sealed record ReviewUpdatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid ReviewId) : IIntegrationEvent;

public sealed record ReviewDeletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid ReviewId) : IIntegrationEvent;

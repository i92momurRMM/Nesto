namespace Nesto.Contracts.IntegrationEvents.V1;

public sealed record UserRegisteredIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid UserId) : IIntegrationEvent;

public sealed record UserProfileUpdatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid UserId) : IIntegrationEvent;

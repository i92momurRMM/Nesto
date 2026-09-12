namespace Nesto.Contracts.IntegrationEvents.V1;

public sealed record BookingReservedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid BookingId) : IIntegrationEvent;

public sealed record BookingConfirmedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid BookingId) : IIntegrationEvent;

public sealed record BookingRejectedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid BookingId) : IIntegrationEvent;

public sealed record BookingCompletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid BookingId) : IIntegrationEvent;

public sealed record BookingCancelledIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid BookingId) : IIntegrationEvent;

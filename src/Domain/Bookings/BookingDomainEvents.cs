using Nesto.SharedKernel;

namespace Nesto.Domain.Bookings;

public sealed record BookingReservedDomainEvent(Guid BookingId) : IDomainEvent;

public sealed record BookingConfirmedDomainEvent(Guid BookingId) : IDomainEvent;

public sealed record BookingRejectedDomainEvent(Guid BookingId) : IDomainEvent;

public sealed record BookingCompletedDomainEvent(Guid BookingId) : IDomainEvent;

public sealed record BookingCancelledDomainEvent(Guid BookingId) : IDomainEvent;

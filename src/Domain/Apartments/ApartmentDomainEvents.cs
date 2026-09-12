using Nesto.SharedKernel;

namespace Nesto.Domain.Apartments;

public sealed record ApartmentCreatedDomainEvent(Guid ApartmentId, Guid OwnerId) : IDomainEvent;

public sealed record ApartmentUpdatedDomainEvent(Guid ApartmentId, Guid OwnerId) : IDomainEvent;

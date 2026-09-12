namespace Nesto.Application.Reviews;

public sealed record ReviewResponse(
    Guid Id,
    Guid ApartmentId,
    Guid BookingId,
    Guid UserId,
    int Rating,
    string Comment,
    DateTime CreatedOnUtc);

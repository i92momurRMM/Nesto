namespace Nesto.Domain.Reviews;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    void Add(Review review);
    void Remove(Review review);
}

using Nesto.Domain.Reviews;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Reviews;

internal sealed class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Reviews.SingleOrDefaultAsync(review => review.Id == id, cancellationToken);

    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        context.Reviews.AnyAsync(review => review.BookingId == bookingId, cancellationToken);

    public void Add(Review review) => context.Reviews.Add(review);

    public void Remove(Review review) => context.Reviews.Remove(review);
}

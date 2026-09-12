using Nesto.Application.Reviews;
using Nesto.Domain.Reviews;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Nesto.Infrastructure.Reviews;

internal sealed class ReviewReadService(ApplicationDbContext context) : IReviewReadService
{
    public async Task<IReadOnlyList<ReviewResponse>> GetAllAsync(
        Guid? apartmentId,
        CancellationToken cancellationToken)
    {
        IQueryable<Review> query = context.Reviews.AsNoTracking();

        if (apartmentId.HasValue)
        {
            query = query.Where(review => review.ApartmentId == apartmentId.Value);
        }

        List<Review> reviews = await query
            .OrderByDescending(review => review.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        return
        [
            .. reviews.Select(review => new ReviewResponse(
                review.Id,
                review.ApartmentId,
                review.BookingId,
                review.UserId,
                review.Rating.Value,
                review.Comment.Value,
                review.CreatedOnUtc))
        ];
    }
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Reviews;
using Nesto.SharedKernel;

namespace Nesto.Application.Reviews.GetAll;

internal sealed class GetReviewsQueryHandler(IReviewReadService readService)
    : IQueryHandler<GetReviewsQuery, IReadOnlyList<ReviewResponse>>
{
    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(
        GetReviewsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ReviewResponse> reviews = await readService.GetAllAsync(
            query.ApartmentId,
            cancellationToken);
        return Result.Success(reviews);
    }
}

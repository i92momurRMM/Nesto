using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Reviews;
using Nesto.Application.Reviews.GetAll;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Reviews;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("reviews", async (
            Guid? apartmentId,
            IQueryHandler<GetReviewsQuery, IReadOnlyList<ReviewResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<IReadOnlyList<ReviewResponse>> result =
                await handler.Handle(new GetReviewsQuery(apartmentId), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reviews);
}

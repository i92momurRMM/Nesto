using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Reviews.Delete;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Reviews;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("reviews/{reviewId:guid}", async (
            Guid reviewId,
            ICommandHandler<DeleteReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeleteReviewCommand(reviewId), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Reviews);
}

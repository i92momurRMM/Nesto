using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Reviews.Update;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Reviews;

internal sealed class Update : IEndpoint
{
    public sealed record Request(int Rating, string Comment);

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("reviews/{reviewId:guid}", async (
            Guid reviewId,
            Request request,
            ICommandHandler<UpdateReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new UpdateReviewCommand(reviewId, request.Rating, request.Comment), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Reviews);
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Reviews.Create;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Reviews;

internal sealed class Create : IEndpoint
{
    public sealed record Request(Guid BookingId, int Rating, string Comment);

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("reviews", async (
            Request request,
            ICommandHandler<CreateReviewCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(
                new CreateReviewCommand(request.BookingId, request.Rating, request.Comment),
                cancellationToken);
            return result.Match(id => Results.Created($"/api/v1/reviews/{id}", new { id }), CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Reviews);
}

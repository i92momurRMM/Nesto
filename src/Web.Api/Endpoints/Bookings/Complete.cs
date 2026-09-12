using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Complete;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Complete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("bookings/{bookingId:guid}/complete", async (
            Guid bookingId,
            ICommandHandler<CompleteBookingCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new CompleteBookingCommand(bookingId), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Bookings);
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Reject;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Reject : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("bookings/{bookingId:guid}/reject", async (
            Guid bookingId,
            ICommandHandler<RejectBookingCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new RejectBookingCommand(bookingId), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Bookings);
}

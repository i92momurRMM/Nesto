using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Confirm;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Confirm : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("bookings/{bookingId:guid}/confirm", async (
            Guid bookingId,
            ICommandHandler<ConfirmBookingCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmBookingCommand(bookingId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Bookings);
    }
}

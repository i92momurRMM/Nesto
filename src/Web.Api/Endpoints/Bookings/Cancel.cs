using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Cancel;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("bookings/{bookingId:guid}/cancel", async (
            Guid bookingId,
            ICommandHandler<CancelBookingCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelBookingCommand(bookingId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Bookings);
    }
}

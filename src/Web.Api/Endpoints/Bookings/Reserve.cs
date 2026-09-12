using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Reserve;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Reserve : IEndpoint
{
    public sealed record Request(Guid ApartmentId, DateOnly StartDate, DateOnly EndDate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("bookings", async (
            Request request,
            ICommandHandler<ReserveBookingCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ReserveBookingCommand(
                request.ApartmentId,
                request.StartDate,
                request.EndDate);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                bookingId => Results.Created($"/bookings/{bookingId}", bookingId),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Bookings);
    }
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Get;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("bookings/{bookingId:guid}", async (
            Guid bookingId,
            IQueryHandler<GetBookingQuery, BookingResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBookingQuery(bookingId);

            Result<BookingResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Bookings);
    }
}

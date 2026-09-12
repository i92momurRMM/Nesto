using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Bookings.Get;
using Nesto.Application.Bookings.GetAll;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Bookings;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("bookings", async (
            IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<IReadOnlyList<BookingResponse>> result =
                await handler.Handle(new GetBookingsQuery(), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Bookings);
    }
}

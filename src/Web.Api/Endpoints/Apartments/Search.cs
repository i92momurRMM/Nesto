using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Apartments.Search;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Apartments;

internal sealed class Search : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("apartments", async (
            DateOnly startDate,
            DateOnly endDate,
            IQueryHandler<SearchApartmentsQuery, IReadOnlyCollection<ApartmentResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchApartmentsQuery(startDate, endDate);

            Result<IReadOnlyCollection<ApartmentResponse>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Apartments);
    }
}

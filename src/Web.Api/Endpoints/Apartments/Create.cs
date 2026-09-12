using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Apartments.Create;
using Nesto.Domain.Apartments;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Apartments;

internal sealed class Create : IEndpoint
{
    public sealed record Request(
        string Name,
        string Description,
        string Country,
        string State,
        string ZipCode,
        string City,
        string Street,
        decimal PriceAmount,
        string PriceCurrency,
        decimal CleaningFeeAmount,
        string CleaningFeeCurrency,
        Amenity[] Amenities);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("apartments", async (
            Request request,
            ICommandHandler<CreateApartmentCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateApartmentCommand(
                request.Name, request.Description, request.Country, request.State,
                request.ZipCode, request.City, request.Street, request.PriceAmount,
                request.PriceCurrency, request.CleaningFeeAmount,
                request.CleaningFeeCurrency, request.Amenities);
            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(id => Results.Created($"/api/v1/apartments/{id}", new { id }), CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Apartments);
    }
}

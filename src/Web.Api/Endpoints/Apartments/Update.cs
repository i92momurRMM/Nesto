using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Apartments.Update;
using Nesto.Domain.Apartments;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Apartments;

internal sealed class Update : IEndpoint
{
    public sealed record Request(
        decimal PriceAmount,
        string PriceCurrency,
        decimal CleaningFeeAmount,
        string CleaningFeeCurrency,
        Amenity[] Amenities);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("apartments/{apartmentId:guid}", async (
            Guid apartmentId,
            Request request,
            ICommandHandler<UpdateApartmentCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateApartmentCommand(
                apartmentId, request.PriceAmount, request.PriceCurrency,
                request.CleaningFeeAmount, request.CleaningFeeCurrency, request.Amenities);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Apartments);
    }
}

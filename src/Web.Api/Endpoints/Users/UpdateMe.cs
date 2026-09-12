using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Users.UpdateProfile;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Users;

internal sealed class UpdateMe : IEndpoint
{
    public sealed record Request(string FirstName, string LastName);

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("users/me", async (
            Request request,
            ICommandHandler<UpdateProfileCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new UpdateProfileCommand(request.FirstName, request.LastName), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Users);
}

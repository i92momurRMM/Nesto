using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Users;
using Nesto.Application.Users.GetById;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Users;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("users/me", async (
            IUserContext userContext,
            IQueryHandler<GetUserByIdQuery, UserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<UserResponse> result =
                await handler.Handle(new GetUserByIdQuery(userContext.UserId), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).RequireAuthorization().WithTags(Tags.Users);
}

using Nesto.Application.Abstractions.Messaging;
using Nesto.Application.Users;
using Nesto.Application.Users.Login;
using Nesto.SharedKernel;
using Nesto.Api.Extensions;
using Nesto.Api.Nesto.Infrastructure;

namespace Nesto.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    public sealed record Request(string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (
            Request request,
            ICommandHandler<LoginUserCommand, AccessTokensResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<AccessTokensResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireRateLimiting(RateLimitingPolicies.Authentication);
    }
}

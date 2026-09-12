using System.Net;
using System.Net.Http.Json;
using Nesto.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Nesto.IntegrationTests.Users;

public sealed class UsersTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_Should_ReturnUserId()
    {
        Guid userId = await RegisterUserAsync(UniqueEmail());

        userId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Login_Should_ReturnAccessAndRefreshTokens()
    {
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        AccessTokens tokens = await LoginAsync(email);

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_ReturnProblem_WhenPasswordIsInvalid()
    {
        string email = UniqueEmail();
        await RegisterUserAsync(email);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "users/login",
            new { email, password = "WrongPassword1" });

        response.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshToken_Should_ReturnNewTokens()
    {
        string email = UniqueEmail();
        await RegisterUserAsync(email);
        AccessTokens tokens = await LoginAsync(email);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "users/refresh-token",
            new { refreshToken = tokens.RefreshToken });

        response.EnsureSuccessStatusCode();
        AccessTokens? rotated = await response.Content.ReadFromJsonAsync<AccessTokens>();
        rotated!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        rotated.RefreshToken.ShouldNotBe(tokens.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_Should_ReturnProblem_WhenTokenIsInvalid()
    {
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "users/refresh-token",
            new { refreshToken = "this-token-does-not-exist" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMe_Should_ReadUserWithEntityFrameworkNoTracking()
    {
        string email = UniqueEmail();
        Guid userId = await RegisterUserAsync(email);
        AccessTokens tokens = await LoginAsync(email);
        Authenticate(tokens.AccessToken);

        UserResponse? user = await HttpClient.GetFromJsonAsync<UserResponse>("users/me");

        user.ShouldNotBeNull();
        user.Id.ShouldBe(userId);
        user.Email.ShouldBe(email);
    }

    [Fact]
    public async Task UserReadService_Should_FindUserByEmailInPostgreSql()
    {
        string email = UniqueEmail();
        Guid userId = await RegisterUserAsync(email);
        using IServiceScope scope = Services.CreateScope();
        IUserReadService readService = scope.ServiceProvider.GetRequiredService<IUserReadService>();

        UserResponse? user = await readService.GetByEmailAsync(email, CancellationToken.None);

        user.ShouldNotBeNull();
        user.Id.ShouldBe(userId);
    }
}

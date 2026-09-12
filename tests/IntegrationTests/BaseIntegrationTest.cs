using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Nesto.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Nesto.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest
{
    private readonly IntegrationTestWebAppFactory _factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        HttpClient = factory.CreateClient();
        HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress!, "api/v1/");
    }

    protected HttpClient HttpClient { get; }

    protected IServiceProvider Services => _factory.Services;

    protected async Task<Guid> SeedApartmentAsync(
        decimal price = 100m,
        decimal cleaningFee = 25m,
        Guid? ownerId = null)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var apartment = Apartment.Create(
            ownerId ?? Guid.Empty,
            Name.Create($"Apartamento {Guid.NewGuid():N}"),
            Description.Create("Sembrado por la suite de integracion."),
            new Address("España", "Madrid", "28004", "Madrid", "Calle de la Palma 1"),
            new Money(price, Currency.Eur),
            new Money(cleaningFee, Currency.Eur),
            [Amenity.WiFi]);

        dbContext.Apartments.Add(apartment);
        await dbContext.SaveChangesAsync();

        return apartment.Id;
    }

    protected sealed record AccessTokens(string AccessToken, string RefreshToken);

    protected static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";

    protected async Task<Guid> RegisterUserAsync(string email)
    {
        var request = new
        {
            email,
            firstName = "Test",
            lastName = "User",
            password = "Password123!"
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("users/register", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected async Task<AccessTokens> LoginAsync(string email)
    {
        var request = new { email, password = "Password123!" };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("users/login", request);
        response.EnsureSuccessStatusCode();

        AccessTokens? tokens = await response.Content.ReadFromJsonAsync<AccessTokens>();

        return tokens!;
    }

    protected async Task<(Guid UserId, AccessTokens Tokens)> RegisterAndLoginAsync()
    {
        string email = UniqueEmail();
        Guid userId = await RegisterUserAsync(email);
        AccessTokens tokens = await LoginAsync(email);

        return (userId, tokens);
    }

    protected void Authenticate(string accessToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}

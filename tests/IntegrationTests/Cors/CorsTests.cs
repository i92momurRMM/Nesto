using System.Net;

namespace Nesto.IntegrationTests.Cors;

public sealed class CorsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Preflight_ShouldAllowConfiguredFrontendOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "apartments");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        HttpResponseMessage response = await HttpClient.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.GetValues("Access-Control-Allow-Origin").Single()
            .ShouldBe("http://localhost:4200");
    }

    [Fact]
    public async Task Preflight_ShouldRejectUnknownOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "apartments");
        request.Headers.Add("Origin", "https://untrusted.example");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        HttpResponseMessage response = await HttpClient.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }
}

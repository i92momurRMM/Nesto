using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Nesto.IntegrationTests.Bookings;

public sealed class BookingsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private static readonly DateOnly Start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
    private static readonly DateOnly End = Start.AddDays(3);

    [Fact]
    public async Task Reserve_DevuelveCreated_YLaReservaSeLeeDespues()
    {
        Guid apartmentId = await SeedApartmentAsync(price: 100m, cleaningFee: 25m);
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });

        reserved.StatusCode.ShouldBe(HttpStatusCode.Created);
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        HttpResponseMessage read = await HttpClient.GetAsync(new Uri($"bookings/{bookingId}", UriKind.Relative));
        read.EnsureSuccessStatusCode();

        BookingResponse? booking = await read.Content.ReadFromJsonAsync<BookingResponse>();

        booking.ShouldNotBeNull();
        booking.UserId.ShouldBe(userId);
        booking.ApartmentId.ShouldBe(apartmentId);
        booking.Status.ShouldBe(1); // Reserved
        booking.TotalPriceAmount.ShouldBe(325m); // Three nights at 100 plus 25 cleaning fee
    }

    [Fact]
    public async Task Reserve_SobreFechasYaOcupadas_DevuelveConflict()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage first = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        first.StatusCode.ShouldBe(HttpStatusCode.Created);

        HttpResponseMessage second = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start.AddDays(2), endDate = End.AddDays(2) });

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reserve_QueEmpiezaElDiaDeSalidaDeOtra_SiSePermite()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        await HttpClient.PostAsJsonAsync("bookings", new { apartmentId, startDate = Start, endDate = End });

        HttpResponseMessage backToBack = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = End, endDate = End.AddDays(2) });

        backToBack.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Reserve_SinAutenticar_DevuelveUnauthorized()
    {
        Guid apartmentId = await SeedApartmentAsync();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reserve_ConApartamentoInexistente_DevuelveNotFound()
    {
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId = Guid.NewGuid(), startDate = Start, endDate = End });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reserve_ConFechasAlReves_DevuelveBadRequest()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = End, endDate = Start });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_SinConfirmarAntes_DevuelveConflict()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        HttpResponseMessage cancelled = await HttpClient.PutAsync(
            new Uri($"bookings/{bookingId}/cancel", UriKind.Relative),
            content: null);

        cancelled.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ConfirmYCancel_DejanLaReservaCancelada()
    {
        (Guid ownerId, AccessTokens owner) = await RegisterAndLoginAsync();
        Guid apartmentId = await SeedApartmentAsync(ownerId: ownerId);
        (_, AccessTokens guest) = await RegisterAndLoginAsync();
        Authenticate(guest.AccessToken);

        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        Authenticate(owner.AccessToken);
        HttpResponseMessage confirmed = await HttpClient.PutAsync(
            new Uri($"bookings/{bookingId}/confirm", UriKind.Relative), content: null);
        confirmed.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        Authenticate(guest.AccessToken);
        HttpResponseMessage cancelled = await HttpClient.PutAsync(
            new Uri($"bookings/{bookingId}/cancel", UriKind.Relative), content: null);
        cancelled.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage read = await HttpClient.GetAsync(
            new Uri($"bookings/{bookingId}", UriKind.Relative));
        BookingResponse? booking = await read.Content.ReadFromJsonAsync<BookingResponse>();

        booking!.Status.ShouldBe(4); // Cancelled
    }

    [Fact]
    public async Task Confirm_ByGuest_ReturnsOwnerSpecificForbiddenError()
    {
        (Guid ownerId, _) = await RegisterAndLoginAsync();
        Guid apartmentId = await SeedApartmentAsync(ownerId: ownerId);
        (_, AccessTokens guest) = await RegisterAndLoginAsync();
        Authenticate(guest.AccessToken);

        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        HttpResponseMessage confirmed = await HttpClient.PutAsync(
            new Uri($"bookings/{bookingId}/confirm", UriKind.Relative), content: null);

        confirmed.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        ProblemDetails? problem = await confirmed.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Bookings.NotApartmentOwner");
        problem.Detail.ShouldBe("Only the apartment owner can manage this booking.");
    }

    [Fact]
    public async Task Get_DeUnaReservaAjena_DevuelveUnauthorized()
    {
        Guid apartmentId = await SeedApartmentAsync();

        (_, AccessTokens owner) = await RegisterAndLoginAsync();
        Authenticate(owner.AccessToken);

        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        (_, AccessTokens intruder) = await RegisterAndLoginAsync();
        Authenticate(intruder.AccessToken);

        HttpResponseMessage read = await HttpClient.GetAsync(
            new Uri($"bookings/{bookingId}", UriKind.Relative));

        read.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task Search_NoDevuelveApartamentosOcupados()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (_, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);

        HttpResponseMessage libreAntes = await HttpClient.GetAsync(
            new Uri($"apartments?startDate={Start:yyyy-MM-dd}&endDate={End:yyyy-MM-dd}", UriKind.Relative));
        libreAntes.EnsureSuccessStatusCode();

        List<ApartmentSearchResponse> antes =
            (await libreAntes.Content.ReadFromJsonAsync<List<ApartmentSearchResponse>>())!;
        antes.ShouldContain(a => a.Id == apartmentId);

        await HttpClient.PostAsJsonAsync("bookings", new { apartmentId, startDate = Start, endDate = End });

        HttpResponseMessage libreDespues = await HttpClient.GetAsync(
            new Uri($"apartments?startDate={Start:yyyy-MM-dd}&endDate={End:yyyy-MM-dd}", UriKind.Relative));

        List<ApartmentSearchResponse> despues =
            (await libreDespues.Content.ReadFromJsonAsync<List<ApartmentSearchResponse>>())!;
        despues.ShouldNotContain(a => a.Id == apartmentId);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyCurrentUserBookings()
    {
        Guid apartmentId = await SeedApartmentAsync();
        (Guid userId, AccessTokens tokens) = await RegisterAndLoginAsync();
        Authenticate(tokens.AccessToken);
        HttpResponseMessage reserved = await HttpClient.PostAsJsonAsync(
            "bookings",
            new { apartmentId, startDate = Start, endDate = End });
        Guid bookingId = await reserved.Content.ReadFromJsonAsync<Guid>();

        List<BookingResponse>? bookings = await HttpClient.GetFromJsonAsync<List<BookingResponse>>("bookings");

        bookings.ShouldNotBeNull();
        BookingResponse booking = bookings.ShouldHaveSingleItem();
        booking.Id.ShouldBe(bookingId);
        booking.UserId.ShouldBe(userId);
    }

    private sealed record BookingResponse(
        Guid Id,
        Guid ApartmentId,
        Guid UserId,
        int Status,
        decimal TotalPriceAmount);

    private sealed record ApartmentSearchResponse(Guid Id, string Name);
}

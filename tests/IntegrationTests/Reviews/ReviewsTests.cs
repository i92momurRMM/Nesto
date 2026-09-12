using System.Net.Http.Json;
using Nesto.Application.Reviews;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Reviews;
using Nesto.Domain.Shared;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nesto.IntegrationTests.Reviews;

public sealed class ReviewsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetReviews_FiltersByApartmentAndMapsValueObjects()
    {
        (Guid userId, _) = await RegisterAndLoginAsync();
        Guid requestedApartmentId = await SeedApartmentAsync();
        Guid otherApartmentId = await SeedApartmentAsync();
        Guid reviewId = await SeedReviewAsync(requestedApartmentId, userId, 5, "Excellent stay");
        await SeedReviewAsync(otherApartmentId, userId, 2, "Not for me");

        List<ReviewResponse>? reviews = await HttpClient.GetFromJsonAsync<List<ReviewResponse>>(
            $"reviews?apartmentId={requestedApartmentId}");

        reviews.ShouldNotBeNull();
        ReviewResponse review = reviews.ShouldHaveSingleItem();
        review.Id.ShouldBe(reviewId);
        review.ApartmentId.ShouldBe(requestedApartmentId);
        review.Rating.ShouldBe(5);
        review.Comment.ShouldBe("Excellent stay");
    }

    private async Task<Guid> SeedReviewAsync(
        Guid apartmentId,
        Guid userId,
        int ratingValue,
        string commentValue)
    {
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Apartment apartment = await context.Apartments.SingleAsync(candidate => candidate.Id == apartmentId);
        DateTime now = DateTime.UtcNow;
        var duration = DateRange.Create(
            DateOnly.FromDateTime(now).AddDays(-3),
            DateOnly.FromDateTime(now).AddDays(-1));
        var booking = Booking.Reserve(apartment, userId, duration, now.AddDays(-4), new PricingService());
        booking.Confirm(now.AddDays(-4));
        booking.Complete(now);
        Review review = Review.Create(
            booking,
            Rating.Create(ratingValue).Value,
            Comment.Create(commentValue).Value,
            now).Value;

        context.Bookings.Add(booking);
        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        return review.Id;
    }
}

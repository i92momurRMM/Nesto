using Nesto.Domain.Bookings;
using Nesto.SharedKernel;

namespace Nesto.Domain.Reviews;

public sealed class Review : Entity
{
    private Review()
    {
    }

    private Review(Guid id, Booking booking, Rating rating, Comment comment, DateTime createdOnUtc)
    {
        Id = id;
        ApartmentId = booking.ApartmentId;
        BookingId = booking.Id;
        UserId = booking.UserId;
        Rating = rating;
        Comment = comment;
        CreatedOnUtc = createdOnUtc;
    }

    public Guid Id { get; private set; }
    public Guid ApartmentId { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public Rating Rating { get; private set; }
    public Comment Comment { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }

    public static Result<Review> Create(Booking booking, Rating rating, Comment comment, DateTime createdOnUtc)
    {
        if (booking.Status != BookingStatus.Completed)
        {
            return Result.Failure<Review>(ReviewErrors.NotEligible);
        }

        var review = new Review(Guid.CreateVersion7(), booking, rating, comment, createdOnUtc);
        review.Raise(new ReviewCreatedDomainEvent(review.Id));
        return review;
    }

    public void Update(Rating rating, Comment comment)
    {
        Rating = rating;
        Comment = comment;
        Raise(new ReviewUpdatedDomainEvent(Id));
    }

    public void MarkDeleted() => Raise(new ReviewDeletedDomainEvent(Id));
}

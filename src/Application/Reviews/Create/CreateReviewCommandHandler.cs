using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Bookings;
using Nesto.Domain.Reviews;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Reviews.Create;

internal sealed class CreateReviewCommandHandler(
    IBookingRepository bookingRepository,
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IDateTimeProvider clock) : ICommandHandler<CreateReviewCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure<Guid>(BookingErrors.NotFound(command.BookingId));
        }

        if (booking.UserId != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized());
        }

        if (await reviewRepository.ExistsForBookingAsync(command.BookingId, cancellationToken))
        {
            return Result.Failure<Guid>(ReviewErrors.AlreadyExists);
        }

        Result<Rating> rating = Rating.Create(command.Rating);
        Result<Comment> comment = Comment.Create(command.Comment);
        if (rating.IsFailure)
        {
            return Result.Failure<Guid>(rating.Error);
        }

        if (comment.IsFailure)
        {
            return Result.Failure<Guid>(comment.Error);
        }

        Result<Review> review = Review.Create(booking, rating.Value, comment.Value, clock.UtcNow);
        if (review.IsFailure)
        {
            return Result.Failure<Guid>(review.Error);
        }

        reviewRepository.Add(review.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return review.Value.Id;
    }
}

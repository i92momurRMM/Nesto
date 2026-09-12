using Nesto.SharedKernel;

namespace Nesto.Domain.Reviews;

public static class ReviewErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("Reviews.NotFound", $"Review '{id}' was not found.");

    public static readonly Error NotEligible = Error.Conflict(
        "Reviews.NotEligible",
        "Only completed bookings can be reviewed.");

    public static readonly Error InvalidRating = Error.Problem(
        "Reviews.InvalidRating",
        "Rating must be between 1 and 5.");

    public static readonly Error InvalidComment = Error.Problem(
        "Reviews.InvalidComment",
        "Comment is required and cannot exceed 1000 characters.");

    public static readonly Error AlreadyExists = Error.Conflict(
        "Reviews.AlreadyExists",
        "A review already exists for this booking.");
}

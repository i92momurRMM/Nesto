using Nesto.SharedKernel;

namespace Nesto.Domain.Reviews;

public sealed record Rating
{
    public const int Minimum = 1;
    public const int Maximum = 5;

    private Rating(int value) => Value = value;

    public int Value { get; }

    public static Result<Rating> Create(int value) =>
        value is >= Minimum and <= Maximum
            ? new Rating(value)
            : Result.Failure<Rating>(ReviewErrors.InvalidRating);
}

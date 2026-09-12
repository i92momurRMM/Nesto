using Nesto.SharedKernel;

namespace Nesto.Domain.Reviews;

public sealed record Comment
{
    public const int MaxLength = 1000;

    private Comment(string value) => Value = value;

    public string Value { get; }

    public static Result<Comment> Create(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= MaxLength
            ? new Comment(value.Trim())
            : Result.Failure<Comment>(ReviewErrors.InvalidComment);
}

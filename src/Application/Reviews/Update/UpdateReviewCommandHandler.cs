using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Reviews;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Reviews.Update;

internal sealed class UpdateReviewCommandHandler(
    IReviewRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<UpdateReviewCommand>
{
    public async Task<Result> Handle(UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        Review? review = await repository.GetByIdAsync(command.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.Failure(ReviewErrors.NotFound(command.ReviewId));
        }

        if (review.UserId != userContext.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized());
        }

        Result<Rating> rating = Rating.Create(command.Rating);
        Result<Comment> comment = Comment.Create(command.Comment);
        if (rating.IsFailure)
        {
            return Result.Failure(rating.Error);
        }

        if (comment.IsFailure)
        {
            return Result.Failure(comment.Error);
        }

        review.Update(rating.Value, comment.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

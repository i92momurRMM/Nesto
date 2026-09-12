using Nesto.Application.Abstractions.Authentication;
using Nesto.Application.Abstractions.Data;
using Nesto.Application.Abstractions.Messaging;
using Nesto.Domain.Reviews;
using Nesto.Domain.Users;
using Nesto.SharedKernel;

namespace Nesto.Application.Reviews.Delete;

internal sealed class DeleteReviewCommandHandler(
    IReviewRepository repository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<DeleteReviewCommand>
{
    public async Task<Result> Handle(DeleteReviewCommand command, CancellationToken cancellationToken)
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

        review.MarkDeleted();
        repository.Remove(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

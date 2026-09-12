using Nesto.Domain.Reviews;
using FluentValidation;

namespace Nesto.Application.Reviews.Update;

internal sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(command => command.ReviewId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(Rating.Minimum, Rating.Maximum);
        RuleFor(command => command.Comment).NotEmpty().MaximumLength(Comment.MaxLength);
    }
}

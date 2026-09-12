using Nesto.Domain.Reviews;
using FluentValidation;

namespace Nesto.Application.Reviews.Create;

internal sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(command => command.BookingId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(Rating.Minimum, Rating.Maximum);
        RuleFor(command => command.Comment).NotEmpty().MaximumLength(Comment.MaxLength);
    }
}

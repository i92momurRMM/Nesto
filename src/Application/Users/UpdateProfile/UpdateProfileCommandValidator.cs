using Nesto.Domain.Users;
using FluentValidation;

namespace Nesto.Application.Users.UpdateProfile;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(PersonName.MaxLength);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(PersonName.MaxLength);
    }
}

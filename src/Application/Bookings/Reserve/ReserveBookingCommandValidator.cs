using FluentValidation;

namespace Nesto.Application.Bookings.Reserve;

internal sealed class ReserveBookingCommandValidator : AbstractValidator<ReserveBookingCommand>
{
    public ReserveBookingCommandValidator()
    {
        RuleFor(c => c.ApartmentId).NotEmpty();

        RuleFor(c => c.StartDate)
            .LessThan(c => c.EndDate)
            .WithMessage("The check-in date must be before the check-out date.");
    }
}

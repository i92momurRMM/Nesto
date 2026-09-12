using Nesto.Domain.Shared;
using FluentValidation;

namespace Nesto.Application.Apartments.Update;

internal sealed class UpdateApartmentCommandValidator : AbstractValidator<UpdateApartmentCommand>
{
    public UpdateApartmentCommandValidator()
    {
        RuleFor(command => command.ApartmentId).NotEmpty();
        RuleFor(command => command.PriceAmount).GreaterThanOrEqualTo(0);
        RuleFor(command => command.CleaningFeeAmount).GreaterThanOrEqualTo(0);
        RuleFor(command => command.PriceCurrency)
            .Must(BeSupportedCurrency)
            .WithMessage("The price currency is not supported.");
        RuleFor(command => command.CleaningFeeCurrency)
            .Must(BeSupportedCurrency)
            .WithMessage("The cleaning fee currency is not supported.");
        RuleFor(command => command)
            .Must(command => command.PriceCurrency == command.CleaningFeeCurrency)
            .WithMessage("Price and cleaning fee must use the same currency.");
    }

    private static bool BeSupportedCurrency(string code) =>
        Currency.All.Any(currency => currency.Code == code);
}

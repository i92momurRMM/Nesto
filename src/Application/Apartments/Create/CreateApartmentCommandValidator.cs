using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using FluentValidation;

namespace Nesto.Application.Apartments.Create;

internal sealed class CreateApartmentCommandValidator : AbstractValidator<CreateApartmentCommand>
{
    public CreateApartmentCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Name.MaxLength);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(Description.MaxLength);
        RuleFor(command => command.Country).NotEmpty();
        RuleFor(command => command.State).NotEmpty();
        RuleFor(command => command.ZipCode).NotEmpty();
        RuleFor(command => command.City).NotEmpty();
        RuleFor(command => command.Street).NotEmpty();
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

using Application.Commands.CalculateTax;
using FluentValidation;

namespace Application.Validators;

public class CalculateTaxCommandValidator : AbstractValidator<CalculateTaxCommand>
{
    public CalculateTaxCommandValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty()
            .WithMessage("CityId is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.Timestamps)
            .NotEmpty()
            .WithMessage("Timestamps are required")
            .Must(t => t != null && t.Count() <= 100)
            .WithMessage("Maximum 100 timestamps allowed");
    }
}

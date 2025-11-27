using Application.Commands.UpdateCity;
using FluentValidation;

namespace Application.Validators;

public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("City ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("City name is required")
            .MaximumLength(100)
            .WithMessage("City name must not exceed 100 characters");
    }
}
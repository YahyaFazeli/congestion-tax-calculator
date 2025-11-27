using Application.Commands.CreateCity;
using FluentValidation;

namespace Application.Validators;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    public CreateCityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("City name is required")
            .MaximumLength(100)
            .WithMessage("City name must not exceed 100 characters");
    }
}
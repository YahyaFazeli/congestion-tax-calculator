using Application.Commands.AddCityTaxRule;
using FluentValidation;

namespace Application.Validators;

public class AddCityTaxRuleCommandValidator : AbstractValidator<AddCityTaxRuleCommand>
{
    public AddCityTaxRuleCommandValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty()
            .WithMessage("City ID is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.DailyMax)
            .GreaterThan(0)
            .WithMessage("Daily maximum must be greater than 0");

        RuleFor(x => x.SingleChargeMinutes)
            .GreaterThan(0)
            .WithMessage("Single charge minutes must be greater than 0");

        RuleFor(x => x.Intervals)
            .NotEmpty()
            .WithMessage("At least one toll interval is required")
            .Must(intervals => intervals.All(i => TimeSpan.TryParse(i.Start, out _) && TimeSpan.TryParse(i.End, out _)))
            .WithMessage("All intervals must have valid start and end times")
            .Must(intervals => intervals.All(i => TimeSpan.Parse(i.Start) < TimeSpan.Parse(i.End)))
            .WithMessage("Start time must be before end time for all intervals")
            .Must(intervals => intervals.All(i => i.Amount > 0))
            .WithMessage("All interval amounts must be greater than 0");

        RuleFor(x => x.FreeDates)
            .Must(dates => dates == null || dates.All(d => DateTime.TryParse(d.Date, out _)))
            .WithMessage("All free dates must be valid dates");

        RuleFor(x => x.FreeMonths)
            .Must(months => months == null || months.Distinct().Count() == months.Count())
            .WithMessage("Free months must be unique");

        RuleFor(x => x.FreeWeekdays)
            .Must(days => days == null || days.Distinct().Count() == days.Count())
            .WithMessage("Free weekdays must be unique");

        RuleFor(x => x.FreeVehicles)
            .Must(vehicles => vehicles == null || vehicles.Distinct().Count() == vehicles.Count())
            .WithMessage("Free vehicle types must be unique");
    }
}
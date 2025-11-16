using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Commands.AddCityTaxRule;

public sealed class AddCityTaxRuleCommandHandler(ICityRepository cityRepository)
    : IRequestHandler<AddCityTaxRuleCommand, AddCityTaxRuleResult>
{
    public async Task<AddCityTaxRuleResult> Handle(
        AddCityTaxRuleCommand request,
        CancellationToken cancellationToken
    )
    {
        var city = await cityRepository.GetByIdWithRulesAsync(request.CityId, cancellationToken);

        if (city is null)
            throw new InvalidOperationException($"City with ID '{request.CityId}' not found");

        var existingRule = city.GetRuleForYear(request.Year);
        if (existingRule is not null)
            throw new InvalidOperationException(
                $"Tax rule for year {request.Year} already exists for city '{city.Name}'"
            );

        var intervals = request.Intervals.Select(i =>
            TollInterval.Create(TimeOnly.Parse(i.Start), TimeOnly.Parse(i.End), new Money(i.Amount))
        );

        var freeDates = request.FreeDates.Select(d =>
            TollFreeDate.Create(DateOnly.Parse(d.Date), d.IncludeDayBefore)
        );

        var freeMonths = request.FreeMonths.Select(m => TollFreeMonth.Create(m));

        var freeWeekdays = request.FreeWeekdays.Select(w => TollFreeWeekday.Create(w));

        var freeVehicles = request.FreeVehicles.Select(v => TollFreeVehicle.Create(v));

        var taxRule = TaxRule.Create(
            request.CityId,
            request.Year,
            new Money(request.DailyMax),
            request.SingleChargeMinutes,
            intervals,
            freeDates,
            freeMonths,
            freeWeekdays,
            freeVehicles
        );

        await cityRepository.AddTaxRuleAsync(taxRule, cancellationToken);

        return new AddCityTaxRuleResult(taxRule.Id, city.Id, request.Year);
    }
}

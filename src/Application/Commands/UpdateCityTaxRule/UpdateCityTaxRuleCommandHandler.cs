using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Commands.UpdateCityTaxRule;

public sealed class UpdateCityTaxRuleCommandHandler(ITaxRuleRepository taxRuleRepository)
    : IRequestHandler<UpdateCityTaxRuleCommand, UpdateCityTaxRuleResult>
{
    public async Task<UpdateCityTaxRuleResult> Handle(
        UpdateCityTaxRuleCommand request,
        CancellationToken cancellationToken
    )
    {
        var existingRule = await taxRuleRepository.GetByIdWithAllRelationsAsync(
            request.RuleId,
            cancellationToken
        );

        if (existingRule is null)
            throw new InvalidOperationException($"Tax rule with ID '{request.RuleId}' not found");

        if (existingRule.CityId != request.CityId)
            throw new InvalidOperationException(
                $"Tax rule '{request.RuleId}' does not belong to city '{request.CityId}'"
            );

        if (existingRule.Year != request.Year)
        {
            var duplicateRule = await taxRuleRepository.GetByCityAndYearAsync(
                request.CityId,
                request.Year,
                cancellationToken
            );

            if (duplicateRule is not null)
                throw new InvalidOperationException(
                    $"Another tax rule for year {request.Year} already exists for this city"
                );
        }

        var intervals = request.Intervals.Select(i =>
            TollInterval.Create(TimeOnly.Parse(i.Start), TimeOnly.Parse(i.End), new Money(i.Amount))
        );

        var freeDates = request.FreeDates.Select(d =>
            TollFreeDate.Create(DateOnly.Parse(d.Date), d.IncludeDayBefore)
        );

        var freeMonths = request.FreeMonths.Select(m => TollFreeMonth.Create(m));

        var freeWeekdays = request.FreeWeekdays.Select(w => TollFreeWeekday.Create(w));

        var freeVehicles = request.FreeVehicles.Select(v => TollFreeVehicle.Create(v));

        var updatedRule = TaxRule.Create(
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

        await taxRuleRepository.ReplaceRuleAsync(request.RuleId, updatedRule, cancellationToken);

        return new UpdateCityTaxRuleResult(updatedRule.Id, request.CityId, request.Year);
    }
}

using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.UpdateCityTaxRule;

public sealed class UpdateCityTaxRuleCommandHandler(
    ITaxRuleRepository taxRuleRepository,
    ILogger<UpdateCityTaxRuleCommandHandler> logger
) : IRequestHandler<UpdateCityTaxRuleCommand, UpdateCityTaxRuleResult>
{
    public async Task<UpdateCityTaxRuleResult> Handle(
        UpdateCityTaxRuleCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Updating tax rule. RuleId: {RuleId}, CityId: {CityId}, Year: {Year}",
            request.RuleId,
            request.CityId,
            request.Year
        );

        var existingRule = await taxRuleRepository.GetByIdWithAllRelationsAsync(
            request.RuleId,
            cancellationToken
        );

        if (existingRule is null)
        {
            throw new TaxRuleNotFoundException(request.CityId, request.Year);
        }

        if (existingRule.CityId != request.CityId)
        {
            throw new ValidationException(
                $"Tax rule '{request.RuleId}' does not belong to city '{request.CityId}'",
                new Dictionary<string, string[]>
                {
                    {
                        nameof(request.CityId),
                        new[] { "Tax rule does not belong to the specified city" }
                    },
                }
            );
        }

        if (existingRule.Year != request.Year)
        {
            var duplicateRule = await taxRuleRepository.GetByCityAndYearAsync(
                request.CityId,
                request.Year,
                cancellationToken
            );

            if (duplicateRule is not null)
            {
                throw new ValidationException(
                    $"Another tax rule for year {request.Year} already exists for this city",
                    new Dictionary<string, string[]>
                    {
                        {
                            nameof(request.Year),
                            new[] { $"Tax rule for year {request.Year} already exists" }
                        },
                    }
                );
            }
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
        await taxRuleRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Tax rule updated successfully. RuleId: {RuleId}, CityId: {CityId}, Year: {Year}",
            updatedRule.Id,
            request.CityId,
            request.Year
        );

        return new UpdateCityTaxRuleResult(updatedRule.Id, request.CityId, request.Year);
    }
}

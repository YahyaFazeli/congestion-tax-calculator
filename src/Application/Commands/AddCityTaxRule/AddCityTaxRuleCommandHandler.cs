using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.AddCityTaxRule;

public sealed class AddCityTaxRuleCommandHandler(
    ICityRepository cityRepository,
    ILogger<AddCityTaxRuleCommandHandler> logger
) : IRequestHandler<AddCityTaxRuleCommand, Result<AddCityTaxRuleResult>>
{
    public async Task<Result<AddCityTaxRuleResult>> Handle(
        AddCityTaxRuleCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Adding tax rule for CityId: {CityId}, Year: {Year}",
            request.CityId,
            request.Year
        );

        var city = await cityRepository.GetByIdWithRulesAsync(request.CityId, cancellationToken);

        if (city is null)
        {
            logger.LogWarning("City not found. CityId: {CityId}", request.CityId);
            return Result.Failure<AddCityTaxRuleResult>(Errors.City.NotFound(request.CityId));
        }

        var existingRule = city.GetRuleForYear(request.Year);
        if (existingRule is not null)
        {
            logger.LogWarning(
                "Tax rule for year already exists. CityId: {CityId}, Year: {Year}",
                request.CityId,
                request.Year
            );
            return Result.Failure<AddCityTaxRuleResult>(Errors.TaxRule.AlreadyExists(request.Year));
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
        await cityRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Tax rule added successfully. RuleId: {RuleId}, CityId: {CityId}, Year: {Year}",
            taxRule.Id,
            city.Id,
            request.Year
        );

        return Result.Success(new AddCityTaxRuleResult(taxRule.Id, city.Id, request.Year));
    }
}

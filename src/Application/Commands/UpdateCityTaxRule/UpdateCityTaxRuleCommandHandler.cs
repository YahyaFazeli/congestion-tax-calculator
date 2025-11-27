using Domain.Common;
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
) : IRequestHandler<UpdateCityTaxRuleCommand, Result<UpdateCityTaxRuleResult>>
{
    public async Task<Result<UpdateCityTaxRuleResult>> Handle(
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
            logger.LogWarning(
                "Tax rule not found. RuleId: {RuleId}, CityId: {CityId}, Year: {Year}",
                request.RuleId,
                request.CityId,
                request.Year
            );
            return Result.Failure<UpdateCityTaxRuleResult>(
                Errors.TaxRule.NotFound(request.CityId, request.Year)
            );
        }

        if (existingRule.CityId != request.CityId)
        {
            logger.LogWarning(
                "Tax rule belongs to different city. RuleId: {RuleId}, ExpectedCityId: {ExpectedCityId}, ActualCityId: {ActualCityId}",
                request.RuleId,
                request.CityId,
                existingRule.CityId
            );
            return Result.Failure<UpdateCityTaxRuleResult>(
                Errors.TaxRule.WrongCity(request.RuleId, request.CityId)
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
                logger.LogWarning(
                    "Tax rule for year already exists. CityId: {CityId}, Year: {Year}",
                    request.CityId,
                    request.Year
                );
                return Result.Failure<UpdateCityTaxRuleResult>(
                    Errors.TaxRule.AlreadyExists(request.Year)
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

        return Result.Success(
            new UpdateCityTaxRuleResult(updatedRule.Id, request.CityId, request.Year)
        );
    }
}

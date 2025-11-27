using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.GetCityTaxRules;

public sealed class GetCityTaxRuleQueryHandler(
    ICityRepository cityRepository,
    ILogger<GetCityTaxRuleQueryHandler> logger
) : IRequestHandler<GetCityTaxRuleQuery, CityTaxRuleDto?>
{
    public async Task<CityTaxRuleDto?> Handle(
        GetCityTaxRuleQuery request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Retrieving detailed tax rule for CityId: {CityId}, RuleId: {RuleId}",
            request.CityId,
            request.RuleId
        );

        try
        {
            var city = await cityRepository.GetByIdWithDetailedRulesAsync(
                request.CityId,
                request.RuleId,
                cancellationToken
            );

            if (city is null)
            {
                logger.LogWarning(
                    "City not found. CityId: {CityId}, RuleId: {RuleId}",
                    request.CityId,
                    request.RuleId
                );
                return null;
            }

            var rules = city
                .TaxRules.Select(rule => new TaxRuleDto(
                    rule.Id,
                    rule.Year,
                    rule.DailyMax.Value,
                    rule.SingleChargeMinutes,
                    [.. rule.Intervals],
                    [.. rule.TollFreeDates],
                    [.. rule.TollFreeMonths],
                    [.. rule.TollFreeWeekdays],
                    [.. rule.TollFreeVehicles]
                ))
                .ToList();

            logger.LogInformation(
                "Retrieved detailed tax rule for CityId: {CityId}, RuleId: {RuleId}",
                request.CityId,
                request.RuleId
            );

            return new CityTaxRuleDto(city.Id, city.Name, rules);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving detailed tax rule for CityId: {CityId}, RuleId: {RuleId}",
                request.CityId,
                request.RuleId
            );
            throw;
        }
    }
}

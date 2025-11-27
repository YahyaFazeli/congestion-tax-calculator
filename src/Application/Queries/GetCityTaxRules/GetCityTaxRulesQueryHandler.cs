using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.GetCityTaxRules;

public sealed class GetCityTaxRulesQueryHandler(
    ICityRepository cityRepository,
    ILogger<GetCityTaxRulesQueryHandler> logger
) : IRequestHandler<GetCityTaxRulesQuery, CityTaxRulesDto?>
{
    public async Task<CityTaxRulesDto?> Handle(
        GetCityTaxRulesQuery request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Retrieving tax rules for CityId: {CityId}", request.CityId);

        try
        {
            var city = await cityRepository.GetByIdWithRulesAsync(
                request.CityId,
                cancellationToken
            );

            if (city is null)
            {
                logger.LogWarning("City not found. CityId: {CityId}", request.CityId);
                return null;
            }

            var rules = city
                .TaxRules.Select(rule => new TaxRulesDto(
                    rule.Id,
                    rule.Year,
                    rule.DailyMax.Value,
                    rule.SingleChargeMinutes
                ))
                .ToList();

            logger.LogInformation(
                "Retrieved {RuleCount} tax rules for CityId: {CityId}",
                rules.Count,
                request.CityId
            );

            return new CityTaxRulesDto(city.Id, city.Name, rules);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tax rules for CityId: {CityId}", request.CityId);
            throw;
        }
    }
}

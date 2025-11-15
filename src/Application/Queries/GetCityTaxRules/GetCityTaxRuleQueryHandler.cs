using Domain.Interfaces;
using MediatR;

namespace Application.Queries.GetCityTaxRules;

public sealed class GetCityTaxRuleQueryHandler(ICityRepository cityRepository)
    : IRequestHandler<GetCityTaxRuleQuery, CityTaxRuleDto?>
{
    public async Task<CityTaxRuleDto?> Handle(
        GetCityTaxRuleQuery request,
        CancellationToken cancellationToken
    )
    {
        var city = await cityRepository.GetByIdWithDetailedRulesAsync(
            request.CityId,
            request.RuleId,
            cancellationToken
        );

        if (city is null)
            return null;

        var rules = city.TaxRules.Select(rule => new TaxRuleDto(
            rule.Id,
            rule.Year,
            rule.DailyMax.Value,
            rule.SingleChargeMinutes,
            [.. rule.Intervals],
            [.. rule.TollFreeDates],
            [.. rule.TollFreeMonths],
            [.. rule.TollFreeWeekdays],
            [.. rule.TollFreeVehicles]
        ));

        return new CityTaxRuleDto(city.Id, city.Name, rules);
    }
}

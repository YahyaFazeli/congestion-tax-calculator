using Domain.Interfaces;
using MediatR;

namespace Application.Queries.GetCityTaxRules;

public sealed class GetCityTaxRulesQueryHandler(ICityRepository cityRepository)
    : IRequestHandler<GetCityTaxRulesQuery, CityTaxRulesDto?>
{
    public async Task<CityTaxRulesDto?> Handle(
        GetCityTaxRulesQuery request,
        CancellationToken cancellationToken
    )
    {
        var city = await cityRepository.GetByIdWithRulesAsync(request.CityId, cancellationToken);

        if (city is null)
            return null;

        var rules = city.TaxRules.Select(rule => new TaxRulesDto(
            rule.Id,
            rule.Year,
            rule.DailyMax.Value,
            rule.SingleChargeMinutes
        ));

        return new CityTaxRulesDto(city.Id, city.Name, rules);
    }
}

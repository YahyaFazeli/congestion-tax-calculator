using Domain.Entities;

namespace Application.Queries.GetCityTaxRules;

public sealed record CityTaxRulesDto(Guid CityId, string CityName, IEnumerable<TaxRulesDto> Rules);

public sealed record TaxRulesDto(Guid Id, int Year, decimal DailyMax, int SingleChargeMinutes);
